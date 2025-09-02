using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManager : IManager
{
    public string CurrentSceneName = Scenes.NONE;

    private const string LOADING_SCENE_NAME = "00_LoadingScene";

    private LoadingUI _loading;

    public void Initialize()
    {
        
    }

    public void Release()
    {
        
    }

    public async void LoadScene(string sceneName)
    {
        CurrentSceneName = sceneName;

        await LoadSceneAsyncWithLoadScene(sceneName);
    }

    public async UniTask LoadSceneAsyncWithLoadScene(string sceneName)
    {
        await UnitySceneManager.LoadSceneAsync(LOADING_SCENE_NAME, LoadSceneMode.Additive);

        var mono = Object.FindAnyObjectByType<LoadingUI>();

        if(mono is LoadingUI loadingScript)
        {
            _loading = loadingScript;
        }

        if(null == _loading)
        {
            Debug.LogError("로딩 씬 내  Loading_UI를 찾지 못함");
            return;
        }

        await PerformSceneTransition(sceneName);
    }

    private async UniTask PerformSceneTransition(string sceneName)
    {
        var currentScene = UnitySceneManager.GetActiveScene().name;
        if(string.IsNullOrEmpty(currentScene) )
        {
            Debug.LogError("현재 활성화 된 씬 이름이 없습니다.");
            return;
        }

        if (UnitySceneManager.GetSceneByName(currentScene).isLoaded)
        {
            var unloadSceneAsync = UnitySceneManager.UnloadSceneAsync(currentScene);

            while(!unloadSceneAsync.isDone)
            {
                UpdateLoadingUI($"{unloadSceneAsync.progress * 100:F0}%", unloadSceneAsync.progress);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
        else
        {
            Debug.LogError($"현재 씬 {currentScene}은 이미 언로드 된 상태입니다.");
        }

        var sceneLoad = UnitySceneManager.LoadSceneAsync(sceneName);
        if(sceneLoad == null)
        {
            Debug.LogError($"씬 {sceneName}을 찾을 수 없습니다.");
            return;
        }

        while (!sceneLoad.isDone)
        {
            UpdateLoadingUI($"{sceneLoad.progress * 100:F0}%", sceneLoad.progress);
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        //씬 활성화 대기
        sceneLoad.allowSceneActivation = true;
        while (!sceneLoad.isDone)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        //프레임 렌더링까지 기다리기
        await UniTask.WaitForEndOfFrame();
    }

    public void UpdateLoadingUI(string text, float progress)
    {
        _loading.UpdateLoadingUI(text, progress);
    }
}

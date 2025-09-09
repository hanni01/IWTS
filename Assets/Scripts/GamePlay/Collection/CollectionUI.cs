using TMPro;
using UnityEngine;

public class CollectionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text CollectedWineNum;    
    

    public void SetCount(int num)
    {
        CollectedWineNum.text = num.ToString();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour
{
    [SerializeField] Image putImage;
    [SerializeField] Image moveImage;
    [SerializeField] Image deleteImage;

    [SerializeField] Color enableColor;
    [SerializeField] Color disableColor;

    
    //색을 모두 비활성화 시킴
    public void DisableAllImage()
    {
        putImage.color = disableColor;
        moveImage.color = disableColor;
        deleteImage.color = disableColor;
    }

    public void OpenPutImage()
    {
        DisableAllImage();
        putImage.color = enableColor;
    }

    public void OpenMoveImage()
    {
        DisableAllImage();
        moveImage.color = enableColor;
    }

    public void OpenDeleteImage()
    {
        DisableAllImage();
        deleteImage.color = enableColor;
    }
}

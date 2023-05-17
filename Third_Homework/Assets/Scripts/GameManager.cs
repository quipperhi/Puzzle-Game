using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject Puzzle;
    [SerializeField] GameObject Button1;
    [SerializeField] GameObject Button2;
    [SerializeField] GameObject Description;
    [SerializeField] InputField sizeInput;
    private OpenFileName openFileName;
    private Texture2D image;
    private string path;
    private string size;

    public void ReloadScene()
    {
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
    }

    public void OpenDirectory(string type)
    {
        openFileName = new OpenFileName();
        openFileName.structSize = Marshal.SizeOf(openFileName);
        openFileName.filter = "圖片(*.png;*.jpg)\0*.png;*.jpg\0";
        openFileName.file = new string(new char[256]);
        openFileName.maxFile = openFileName.file.Length;
        openFileName.fileTitle = new string(new char[64]);
        openFileName.maxFileTitle = openFileName.fileTitle.Length;
        openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');
        openFileName.title = "選擇圖片";
        openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

        if (LocalDialog.GetSaveFileName(openFileName))
        {
            path = openFileName.file;
            GetImage();
        }
    }

    public void SetImageToPuzzle(ref Texture2D image)
    {
        if (this.image != null)
        {
            image = this.image;
        }
    }

    public void SetDefaultImageToPuzzle()
    {
        this.image = null;
        size = sizeInput.text;
        Button1.SetActive(false);
        Button2.SetActive(false);
        Description.SetActive(false);
        sizeInput.gameObject.SetActive(false);
        Puzzle.SetActive(true);
    }

    public void SetPuzzleSize(ref int size)
    {
        if (Regex.IsMatch(this.size, @"^\d+$"))
        {
            size = int.Parse(this.size);
        }
        else
        {
            return;
        }

        if (size < 3)
        {
            size = 3;
        }
        if (size > 5)
        {
            size = 5;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void GetImage()
    {
        if (path != null && !(path.Length == 0))
        {
            StartCoroutine(DownloadImage(path));
        }
    }

    IEnumerator DownloadImage(string MediaUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            image = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
        size = sizeInput.text;
        Button1.SetActive(false);
        Button2.SetActive(false);
        Description.SetActive(false);
        sizeInput.gameObject.SetActive(false);
        Puzzle.SetActive(true);
    }
}

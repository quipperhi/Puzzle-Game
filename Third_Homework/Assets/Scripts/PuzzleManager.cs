using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PuzzleManager : MonoBehaviour, IPointerDownHandler
{
    [Range(3, 5)][SerializeField] int size;
    [SerializeField] List<AudioClip> swapsoundGroup;
    [SerializeField] AudioClip winSound;
    [SerializeField] GameObject originImage;
    [SerializeField] Button manualshuffleButton;
    [SerializeField] Button solutionButton;
    public Texture2D image;
    public GameObject puzzlepiece;
    private OpenFileName openFileName;
    private List<GameObject> pieces;
    private List<int> solution;
    private int emptyLocation;
    private float gap = 0.1f;
    private bool _hasshuffled = false;
    private bool _shuffling = false;
    private bool _haswon = false;
    private bool _doingsolutiuon = false;

    private void Start()
    {
        solution = new List<int>();
        GameManager gameManager = FindObjectOfType<GameManager>();
        gameManager.SetImageToPuzzle(ref image);
        gameManager.SetPuzzleSize(ref size);
        int width = image.width / size;
        int height = image.height / size;
        pieces = new List<GameObject>();
        Sprite originSprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.zero);
        originImage.GetComponent<Image>().sprite = originSprite;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Sprite sprite = Sprite.Create(image, new Rect(x * width, (size - 1 - y) * height, width, height), Vector2.zero);
                GameObject piece = Instantiate(puzzlepiece, transform);
                pieces.Add(piece);
                piece.GetComponent<Image>().sprite = sprite;
                piece.name = (x + y * size + 1).ToString();
                piece.transform.localScale = (1 - gap) * Vector3.one;
                RectTransform pieceRectTransform = piece.GetComponent<RectTransform>();
                float pieceWidth = pieceRectTransform.rect.width;
                float pieceHeight = pieceRectTransform.rect.height;
                pieceRectTransform.anchoredPosition = new Vector2((-(size - 1) / 2 * pieceWidth) + x * pieceWidth, ((size - 1) / 2 * pieceHeight) - y * pieceHeight);

                if ((x + 1) == size && (y + 1) == size)
                {
                    piece.SetActive(false);
                    emptyLocation = size * size - 1;
                }
            }
        }

    }

    private void Update()
    {
        if (!_hasshuffled)
        {
            _hasshuffled = true;
            _shuffling = true;

            if (pieces[0].transform.localScale == Vector3.one)
            {
                for (int i = 0; i < pieces.Count; i++)
                {
                    pieces[i].transform.localScale = (1 - gap) * Vector3.one;
                }
            }

            if(pieces[pieces.Count-1].activeSelf && _haswon)
            {
                pieces[pieces.Count-1].SetActive(false);
            }
            StartCoroutine(WaitShuffle(0.5f));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData != null && !_haswon)
        {
            PieceSwap(eventData.pointerCurrentRaycast.gameObject);
            CheckCompletion();
        }
    }

    public void DoSolution()
    {
        solutionButton.interactable = false;
        manualshuffleButton.interactable = false;
        _doingsolutiuon = true;
        StartCoroutine(DoSolutionSlow());
    }

    public void ManualShuffle()
    {
        _hasshuffled = false;
        manualshuffleButton.interactable = false;
    }

    private void PieceSwap(GameObject piece)
    {
        int pieceIndex = pieces.IndexOf(piece);
        if (CheckSwapAndSwap(pieceIndex, -size, size)) { return; }
        if (CheckSwapAndSwap(pieceIndex, size, size)) { return; }
        if (CheckSwapAndSwap(pieceIndex, -1, 0)) { return; }
        if (CheckSwapAndSwap(pieceIndex, 1, size - 1)) { return; }
    }

    private bool CheckSwapAndSwap(int i, int offset, int colcheck)
    {
        if ((i % size != colcheck) && (i + offset) == emptyLocation)
        {
            if (!_doingsolutiuon)
            {
                solution.Add((i + offset));
            }
            else
            {
                solution.RemoveAt((solution.Count - 1));
            }

            (pieces[i], pieces[i + offset]) = (pieces[i + offset], pieces[i]);
            (pieces[i].transform.localPosition, pieces[i + offset].transform.localPosition) = (pieces[i + offset].transform.localPosition, pieces[i].transform.localPosition);
            emptyLocation = i;


            if (!_shuffling)
            {
                int randomnumber = Random.Range(0, 3);
                GetComponent<AudioSource>().PlayOneShot(swapsoundGroup[randomnumber], 10);
            }

            return true;
        }
        return false;
    }

    private void CheckCompletion()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].name != $"{i + 1}")
            {
                return;
            }
        }
        Win();
    }

    private IEnumerator WaitShuffle(float duration)
    {
        yield return new WaitForSeconds(duration);
        Shuffle();
    }

    private void Shuffle()
    {
        int count = 0;
        int last = 0;
        while (count < size * size * size)
        {
            int randomnumber = Random.Range(0, size * size);
            if (randomnumber == last) { continue; }
            last = emptyLocation;

            if (CheckSwapAndSwap(randomnumber, -size, size))
            {
                count++;
            }
            else if (CheckSwapAndSwap(randomnumber, size, size))
            {
                count++;
            }
            else if (CheckSwapAndSwap(randomnumber, -1, 0))
            {
                count++;
            }
            else if (CheckSwapAndSwap(randomnumber, 1, size - 1))
            {
                count++;
            }
        }
        _shuffling = false;
        manualshuffleButton.interactable = true;
        solutionButton.interactable = true;
    }

    private void Win()
    {
        GetComponent<AudioSource>().PlayOneShot(winSound, 10);
        _haswon = true;
        pieces[emptyLocation].gameObject.SetActive(true);
        for (int i = 0; i < pieces.Count; i++)
        {
            pieces[i].transform.localScale = Vector3.one;
        }
    }

    IEnumerator DoSolutionSlow()
    {
        while (solution.Count > 0)
        {
            yield return new WaitForSeconds(0.08f);
            if (CheckSwapAndSwap(solution[solution.Count - 1], -size, size)) { continue; }
            if (CheckSwapAndSwap(solution[solution.Count - 1], size, size)) { continue; }
            if (CheckSwapAndSwap(solution[solution.Count - 1], -1, 0)) { continue; }
            if (CheckSwapAndSwap(solution[solution.Count - 1], 1, size - 1)) { continue; }
        }
        Win();
        emptyLocation = size * size - 1;
        _doingsolutiuon = false;
        manualshuffleButton.interactable = true;
    }
}

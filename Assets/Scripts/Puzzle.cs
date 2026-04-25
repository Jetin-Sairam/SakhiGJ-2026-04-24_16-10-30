using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Puzzle : MonoBehaviour
{
    public List<GameObject> puzzlePieces;
    public float angleTolerance = 5f;

    private int pieceIndex = 0;
    private int previousIndex = 0;
    private bool puzzleSolved = false;

    void Start()
    {
        Debug.Log("Puzzle: Use W/S to select pieces, A/D to rotate.");
        SetAlpha(puzzlePieces[pieceIndex], 0.9f);
    }

    void Update()
    {
        // Stop input once puzzle is solved
        if (puzzleSolved) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (pieceIndex < puzzlePieces.Count - 1)
            {
                previousIndex = pieceIndex;
                pieceIndex++;
                UpdateSelectionAlpha();
            }
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (pieceIndex > 0)
            {
                previousIndex = pieceIndex;
                pieceIndex--;
                UpdateSelectionAlpha();
            }
        }

        if (Input.GetKeyDown(KeyCode.A))
            puzzlePieces[pieceIndex].transform.Rotate(0, 0, -10);
        else if (Input.GetKeyDown(KeyCode.D))
            puzzlePieces[pieceIndex].transform.Rotate(0, 0, 10);

        CheckPuzzle();
    }

    private void UpdateSelectionAlpha()
    {
        SetAlpha(puzzlePieces[previousIndex], 1f);
        SetAlpha(puzzlePieces[pieceIndex], 0.75f);
    }

    private void SetAlpha(GameObject obj, float alpha)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    public void CheckPuzzle()
    {
        int solvedCount = 0;

        foreach (GameObject piece in puzzlePieces)
        {
            float angle = piece.transform.eulerAngles.z;
            bool atZero = angle <= angleTolerance || angle >= (360f - angleTolerance);
            if (atZero)
                solvedCount++;
        }

        if (solvedCount == puzzlePieces.Count)
        {
            puzzleSolved = true;
            Debug.Log("Puzzle Solved!");
            StartCoroutine(LoadSceneAfterDelay());
        }
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("OpenBox");
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Puzzle : MonoBehaviour
{
    public List<GameObject> puzzlePieces;

    // How close to 0/360 degrees counts as "solved" for each piece
    public float angleTolerance = 5f;

    private int pieceIndex = 0;

    void Update()
    {
        // Select which piece to rotate
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (pieceIndex < puzzlePieces.Count - 1)
                pieceIndex++;

            Debug.Log($"Selected piece: {pieceIndex}");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (pieceIndex > 0)
                pieceIndex--;

            Debug.Log($"Selected piece: {pieceIndex}");
        }

        // Rotate selected piece
        if (Input.GetKeyDown(KeyCode.A))
            puzzlePieces[pieceIndex].transform.Rotate(0, 0, -10);
        else if (Input.GetKeyDown(KeyCode.D))
            puzzlePieces[pieceIndex].transform.Rotate(0, 0, 10);

        CheckPuzzle();
    }

    public void CheckPuzzle()
    {
        int solvedCount = 0;

        foreach (GameObject piece in puzzlePieces)
        {
            // eulerAngles.z gives degrees (0–360), not a Quaternion component
            float angle = piece.transform.eulerAngles.z;

            // Normalize: Unity eulerAngles returns 0–360
            // Check if close to 0 or 360 (same position)
            bool atZero = angle <= angleTolerance || angle >= (360f - angleTolerance);

            Debug.Log($"{piece.name} eulerAngle.z: {angle} — solved: {atZero}");

            if (atZero)
                solvedCount++;
        }

        if (solvedCount == puzzlePieces.Count)
        {
            Debug.Log("Puzzle Solved!");
            SceneManager.LoadScene("OpenBox");
        }
    }
}
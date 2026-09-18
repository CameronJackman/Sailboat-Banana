using UnityEngine;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;

public class MatchDetector : MonoBehaviour 
{
    public List<Piece> FindMatches(Piece[,] board, int width, int height)
    {
        List<Piece> matches = new List<Piece>();
        // Horizontal Check for matches

        //gets current position
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; x++)
            {
                Piece first = board[x, y];
                Piece second = board[x+ 1, y];
                Piece third = board[x+ 2, y];

                if (first != null && second != null && third != null &&
                    first.TokenType == second.TokenType &&
                    second.TokenType == third.TokenType)
                {
                    //this passes these matched ones to the AddIfMissing Method
                    AddIfMissing(matches, first);
                    AddIfMissing(matches, second);
                    AddIfMissing(matches, third);
                }
            }
        }

        // Vertical Check for matches

        //gets current position
        for (int x = 0; x <width; x++)
        {
           for (int y = 0; y < height - 2; y++)
            {
                Piece first = board[x, y];
                Piece second = board[x, y + 1];
                Piece third = board[x, y + 2];

                if (first != null && second != null && third != null &&
                    first.TokenType == second.TokenType &&
                    second.TokenType == third.TokenType)
                {
                    //this passes these matched ones to the AddIfMissing Method
                    AddIfMissing(matches, first);
                    AddIfMissing(matches, second);
                    AddIfMissing(matches, third);
                }
            }
        }

        //this completes the list returning all the matched tokens
        return matches;
    }

    //this method adds all the matched pieces into a list (matches)
    private void AddIfMissing(List<Piece> matches, Piece piece)
    {
        if (!matches.Contains(piece))
        {
            matches.Add(piece);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }

    public List<CraftRecipe> recipes;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Проверяет сетку 3x3 и возвращает рецепт, если найден.
    /// </summary>
    public CraftRecipe FindRecipe(Item[] grid)
    {
        if (grid == null || grid.Length != 9) return null;

        Item[] normalized = NormalizeGrid(grid);

        foreach (CraftRecipe recipe in recipes)
        {
            Item[] recipeNormalized = NormalizeGrid(recipe.recipeGrid);
            if (CompareGrids(normalized, recipeNormalized))
                return recipe;
        }
        return null;
    }

    /// <summary>
    /// Нормализует сетку — сдвигает все предметы к левому верхнему углу,
    /// чтобы рецепты работали независимо от позиции в сетке 3x3.
    /// </summary>
    private Item[] NormalizeGrid(Item[] grid)
    {
        // Находим минимальные row и col, где есть предметы
        int minRow = 3, maxRow = -1, minCol = 3, maxCol = -1;

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (grid[row * 3 + col] != null)
                {
                    if (row < minRow) minRow = row;
                    if (row > maxRow) maxRow = row;
                    if (col < minCol) minCol = col;
                    if (col > maxCol) maxCol = col;
                }
            }
        }

        // Если сетка пустая — возвращаем пустую
        if (maxRow == -1) return new Item[9];

        // Создаём смещённую сетку
        Item[] result = new Item[9];
        int height = maxRow - minRow + 1;
        int width = maxCol - minCol + 1;

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                result[row * 3 + col] = grid[(minRow + row) * 3 + (minCol + col)];
            }
        }

        return result;
    }

    private bool CompareGrids(Item[] a, Item[] b)
    {
        for (int i = 0; i < 9; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }
}

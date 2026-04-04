using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable object/Craft Recipe")]
public class CraftRecipe : ScriptableObject
{
    [Tooltip("Сетка рецепта 3x3. null означает пустой слот.")]
    public Item[] recipeGrid = new Item[9];

    [Tooltip("Результат крафта")]
    public Item resultItem;

    [Tooltip("Количество предметов в результате")]
    public int resultCount = 1;

    /// <summary>
    /// Проверяет, совпадает ли переданная сетка с рецептом.
    /// </summary>
    public bool Matches(Item[] grid)
    {
        if (grid == null || grid.Length != 9) return false;

        for (int i = 0; i < 9; i++)
        {
            if (grid[i] != recipeGrid[i])
                return false;
        }
        return true;
    }
}

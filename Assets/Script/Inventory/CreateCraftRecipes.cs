using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor-утилита для создания рецептов крафта.
/// Запуск: Tools → Create Craft Recipes в меню Unity.
/// </summary>
public class CreateCraftRecipes : MonoBehaviour
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Create Craft Recipes")]
    public static void CreateRecipes()
    {
        // Загружаем все предметы
        Item wood = AssetDatabase.LoadAssetAtPath<Item>("Assets/Items/Wood.asset");
        Item stone = AssetDatabase.LoadAssetAtPath<Item>("Assets/Items/Stone.asset");
        Item docka = AssetDatabase.LoadAssetAtPath<Item>("Assets/Items/Docka.asset");
        Item pickAxe = AssetDatabase.LoadAssetAtPath<Item>("Assets/Items/PickAxe.asset");
        Item axe = AssetDatabase.LoadAssetAtPath<Item>("Assets/Items/Axe.asset");

        string folder = "Assets/Items/Recipes";

        // Рецепт 1: 2 Wood → 4 Docka (доски)
        CreateRecipe("Recipe_Boards", docka, 4, new Item[]
        {
            null, null, null,
            null, wood, null,
            null, wood, null
        });

        // Рецепт 2: 3 Wood в ряд → PickAxe (кирка из дерева)
        CreateRecipe("Recipe_WoodPickAxe", pickAxe, 1, new Item[]
        {
            null, null, null,
            wood, wood, wood,
            null, null, null
        });

        // Рецепт 3: 3 Stone в ряд → Axe (каменный топор)
        CreateRecipe("Recipe_StoneAxe", axe, 1, new Item[]
        {
            null, null, null,
            stone, stone, stone,
            null, null, null
        });

        // Рецепт 4: 2 Wood вертикально → палки (Docka x2)
        CreateRecipe("Recipe_Sticks", docka, 2, new Item[]
        {
            null, null, null,
            null, wood, null,
            null, wood, null
        });

        // Рецепт 5: 2 Stone + 3 Wood → кирка
        CreateRecipe("Recipe_StonePickAxe", pickAxe, 1, new Item[]
        {
            stone, stone, stone,
            null, wood, null,
            null, wood, null
        });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Рецепты крафта созданы!");
    }

    private static void CreateRecipe(string name, Item result, int count, Item[] grid)
    {
        string path = $"Assets/Items/Recipes/{name}.asset";
        CraftRecipe recipe = ScriptableObject.CreateInstance<CraftRecipe>();
        recipe.name = name;
        recipe.recipeGrid = grid;
        recipe.resultItem = result;
        recipe.resultCount = count;

        AssetDatabase.CreateAsset(recipe, path);
    }
#endif
}

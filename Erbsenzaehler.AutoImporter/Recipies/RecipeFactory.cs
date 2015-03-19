﻿using System;
using Erbsenzaehler.AutoImporter.Configuration;

namespace Erbsenzaehler.AutoImporter.Recipies
{
    public static class RecipeFactory
    {
        public static AbstractRecipe GetRecipe(ConfigurationContainer configuration)
        {
            AbstractRecipe recipe = null;
            if (configuration.Easybank != null)
            {
                recipe = new EasybankRecipe(configuration.Easybank);
            }

            if (recipe != null)
            {
                if (configuration.Program != null)
                {
                    recipe.SaveScreenshots = configuration.Program.SaveScreenshots;
                }

                return recipe;
            }

            throw new Exception("Unable to extract recipe from configuration.");
        }
    }
}
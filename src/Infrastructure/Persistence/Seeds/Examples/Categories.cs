using Domain.Entities.Examples;

namespace Persistence.Seeds.Examples
{
    internal class Categories
    {
        public List<TestCategory>? CategoriesList { get; set; } = new List<TestCategory>();

        public Categories()
        {
            CategoriesList.Clear();
            CategoriesList = new List<TestCategory>
            {
        new TestCategory{
            Name= "Clothes",
            Image= "https://api.lorem.space/image/fashion?w=640&h=480&r=7018"
          },
        new TestCategory {
            Name = "Electronics",
            Image = "https://api.lorem.space/image/watch?w=640&h=480&r=8257"
          },
        new TestCategory {
            Name = "Furniture",
            Image = "https://api.lorem.space/image/furniture?w=640&h=480&r=4054"
          },
        new TestCategory {
            Name = "Shoes",
            Image = "https://api.lorem.space/image/shoes?w=640&h=480&r=6006"
          },
        new TestCategory {
            Name = "Others",
            Image = "https://api.lorem.space/image?w=640&h=480&r=2375"
          },
        new TestCategory{
            Name = "Libros",
            Image = "https://www.pexels.com/es-es/foto/paris-libros-monitor-coleccion-12799377/"
          },
        new TestCategory {
            Name = "Nueva categoria",
            Image = "https://placeimg.com/640/480/any"
          }
      };
        }

        public List<TestCategory> GetListCategories()
        {
            return CategoriesList!;
        }

    }
}

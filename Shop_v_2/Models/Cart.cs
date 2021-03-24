using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shop_v_2.Models
{
    public class Cart
    {
        // Инициализация корзины

        public Cart()

        {

            CartLines = new List<Products>();

        }

        // Элементы корзины

        public List<Products> CartLines { get; set; }

        // Вычисление итоговой стоимости

        public decimal FinalPrice

        {

            get

            {

                decimal sum = 0;

                foreach (Products product in CartLines)

                {

                    sum += product.price;

                }

                return sum;

            }

        }
    }
}

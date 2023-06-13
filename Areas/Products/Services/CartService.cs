using _06_MvcWeb.Products.Models;
using Newtonsoft.Json;

namespace _06_MvcWeb.Products.Services
{
	public class CartService
	{
		public const string CARTKEY = "cart";
		private readonly HttpContext _context;
		public CartService(IHttpContextAccessor context)
		{
			_context = context.HttpContext;
        }
		public List<CartItem> GetCartItems()
		{

			string jsoncart = _context.Session.GetString(CARTKEY);
			if (jsoncart != null)
			{
				return JsonConvert.DeserializeObject<List<CartItem>>(jsoncart);
			}
			return new List<CartItem>();
		}

		// Xóa cart khỏi session
		public void ClearCart()
		{
			_context.Session.Remove(CARTKEY);
		}

		// Lưu Cart (Danh sách CartItem) vào session
		public void SaveCartSession(List<CartItem> ls)
		{
			string jsoncart = JsonConvert.SerializeObject(ls);
			_context.Session.SetString(CARTKEY, jsoncart);
		}
	}
}

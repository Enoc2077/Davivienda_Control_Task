using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davivienda.Models.Modelos
{
    public class LoginModel
    {
        public class LoginInput
        {
            // Usamos string porque USU_NUM en tu modelo de usuario es string
            public string UsuNum { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
            public bool Exito { get; set; }
            public string? Token { get; set; }
            public string? Mensaje { get; set; }
            public UsuarioModel? Usuario { get; set; } // Opcional, por si quieres mostrar su nombre de inmediato
        }


    }
}

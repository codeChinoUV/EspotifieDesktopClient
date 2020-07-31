using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Api.GrpcClients;
using Model;

namespace Api.Rest.ApiLogin
{
    public class ApiServiceLogin
    {
        private static ApiServiceLogin _loginService;
        private static HttpClient _apiClient;
        private string _autenticationToken = "";
        private Login _userLogin;

        private ApiServiceLogin()
        {
        }

        public Usuario Usuario { get; set; }

        /// <summary>
        ///     Devuelve el token de autenticacion del usuario actual
        /// </summary>
        /// <returns>El token de autenticacion</returns>
        public string GetAccessToken()
        {
            return _autenticationToken;
        }

        /// <summary>
        ///     Vuelve a logear al usuario con los credenciales almacenadas
        /// </summary>
        public async Task ReLogin()
        {
            await Login(_userLogin);
        }

        /// <summary>
        ///     Logea al usuario actual con las credenciales que se encuentran en el objeto lgin
        /// </summary>
        /// <param name="login">El objeto que contiene las credenciales del Usuario</param>
        /// <returns>Task</returns>
        /// <exception cref="Exception">Una Excepcion generada al Logearse</exception>
        public async Task Login(Login login)
        {
            _userLogin = login;
            ErrorGeneral errorGeneral;
            var path = "/v1/login";
            var byteArray = Encoding.ASCII.GetBytes($"{login.User}:{login.Password}");
            _apiClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            using (var response = await _apiClient.GetAsync(path))
            {
                var content = response.Content;
                var status = response.StatusCode;
                if (status == HttpStatusCode.BadRequest)
                {
                    errorGeneral = await response.Content.ReadAsAsync<ErrorGeneral>();
                    throw new Exception(errorGeneral.mensaje);
                }

                if (status == HttpStatusCode.InternalServerError)
                    throw new Exception("Ocurrio un error en el servidor");

                var result = await content.ReadAsStringAsync();
                var splitResult = result.Split(':');
                _autenticationToken = splitResult[1];
                _autenticationToken = _autenticationToken.Replace("\"", "");
                _autenticationToken = _autenticationToken.Replace("}", "");
                _autenticationToken = _autenticationToken.Replace("\n", "");
            }

            _apiClient.DefaultRequestHeaders.Clear();
        }

        /// <summary>
        ///     Recupera la instancia del singleton del ServiceLogin
        /// </summary>
        /// <returns>La instancia del singleton ServiceLogin</returns>
        public static ApiServiceLogin GetServiceLogin()
        {
            if (_loginService == null)
            {
                _loginService = new ApiServiceLogin();
                _apiClient = new HttpClient();
                _apiClient.DefaultRequestHeaders.Accept.Clear();
                _apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _apiClient.BaseAddress = new Uri(Configuration.URIRestServer);
            }

            return _loginService;
        }
    }
}
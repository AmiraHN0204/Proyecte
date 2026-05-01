using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ReVita
{
    public partial class frmPersonal : Form
    {
        Form1 FormularioPrincipal;
        SqlConnection Conexion = new SqlConnection();
        public frmPersonal(Form1 Formulario)
        {
            InitializeComponent();
            FormularioPrincipal = Formulario;
            Conexion = FormularioPrincipal.ObtenerConexion();
        }

        private void frmPersonal_Load(object sender, EventArgs e)
        {
            var (btnInsertar, btnEliminar, btnActualizar, btnConsulta, btnLimpiar) =
                FormularioPrincipal.InicializarModulo(this, "Personal");

            btnInsertar.Click += BtnInsertar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnActualizar.Click += BtnActualizar_Click;
            btnConsulta.Click += BtnConsulta_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
        }

        //Método para leer los valores de los campos del formulario, para así poder reutilizarlo en los distintos eventos de los botones
        private (object IDPersonal, object Nombre, object Direccion, object Telefono, object Poblacion, object Provincia, object CodPostal, object NSS) LeerValorCampo()
        {
            return(
                FormularioPrincipal.LeerValorCampo(this, "ID_Personal", "Personal", FormularioPrincipal.ObtenerTipo("ID_Personal", "Personal")),
                FormularioPrincipal.LeerValorCampo(this, "Nombre", "Personal", FormularioPrincipal.ObtenerTipo("Nombre", "Personal")),
                FormularioPrincipal.LeerValorCampo(this, "Direccion", "Personal", FormularioPrincipal.ObtenerTipo("Direccion", "Personal")),
                FormularioPrincipal.LeerValorCampo(this, "Telefono", "Personal", FormularioPrincipal.ObtenerTipo("Telefono", "Personal")),
                FormularioPrincipal.LeerValorCampo(this, "Poblacion", "Personal", FormularioPrincipal.ObtenerTipo("Poblacion", "Personal")),
                FormularioPrincipal.LeerValorCampo(this, "Provincia", "Personal", FormularioPrincipal.ObtenerTipo("Provincia", "Personal")),
                FormularioPrincipal.LeerValorCampo(this, "Codigo_Postal", "Personal", FormularioPrincipal.ObtenerTipo("Codigo_Postal", "Personal")),
                FormularioPrincipal.LeerValorCampo(this, "NSS", "Personal", FormularioPrincipal.ObtenerTipo("NSS", "Personal"))
            );
        }
        private void BtnInsertar_Click(object sender, EventArgs e)
        {
            var (IDPersonal, Nombre, Direccion, Telefono, Poblacion, Provincia, CodPostal, NSS) = LeerValorCampo(); 

            if (IDPersonal == null || Nombre == null || Direccion == null || Telefono == null ||
                Poblacion == null || Provincia == null || CodPostal == null || NSS == null)
            {
                MessageBox.Show("Complete todos los campos para continuar");
                return;
            }
            else
            {
                string Agregar = $"INSERT INTO Personal (ID_Personal, Nombre, Direccion, Telefono, Poblacion, Provincia, Codigo_Postal, NSS)" +
                    $"VALUES (@ID_Personal, @Nombre, @Direccion, @Telefono, @Poblacion, @Provincia, @Codigo_Postal, @NSS)";
                SqlCommand Comando = new SqlCommand(Agregar, Conexion);

                Comando.Parameters.AddWithValue("@ID_Personal", IDPersonal);
                Comando.Parameters.AddWithValue("@Nombre", Nombre);
                Comando.Parameters.AddWithValue("@Direccion", Direccion);
                Comando.Parameters.AddWithValue("@Telefono", Telefono);
                Comando.Parameters.AddWithValue("@Poblacion", Poblacion);
                Comando.Parameters.AddWithValue("@Provincia", Provincia);
                Comando.Parameters.AddWithValue("@Codigo_Postal", CodPostal);
                Comando.Parameters.AddWithValue("@NSS", NSS);

                try
                {
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                
                    Comando.ExecuteNonQuery();
                    FormularioPrincipal.CargarDatos("Personal", this);
                    MessageBox.Show("Registro insertado correctamente");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al insertar el registro: {ex.Message}");
                }
                finally
                {
                    if (Conexion.State == ConnectionState.Open) Conexion.Close();
                }
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var (IDPersonal, _, _, _, _, _, _, _) = LeerValorCampo();

            string Eliminar = $"DELETE FROM Personal WHERE ID_Personal = @ID_Personal"; 
            SqlCommand Comando = new SqlCommand(Eliminar, Conexion);

            Comando.Parameters.AddWithValue("@ID_Personal", IDPersonal);

            try
            {
                if (Conexion.State != ConnectionState.Open) Conexion.Open();

                Comando.ExecuteNonQuery();
                FormularioPrincipal.CargarDatos("Personal", this);
                MessageBox.Show("Registro eliminado correctamente");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar el registro: {ex.Message}");
            }
            finally
            {
                if (Conexion.State == ConnectionState.Open) Conexion.Close();
            }

        }

        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            var (IDPersonal, Nombre, Direccion, Telefono, Poblacion, Provincia, CodPostal, NSS) = LeerValorCampo();

            if (IDPersonal == null || Nombre == null || Direccion == null || Telefono == null ||
                Poblacion == null || Provincia == null || CodPostal == null || NSS == null)
            {
                MessageBox.Show("Complete todos los campos para continuar");
                return;
            }
            else
            {
                string Modificar = $"UPDATE Personal SET ID_Personal = @ID_Personal, Nombre = @Nombre, Direccion = @Direccion, " +
                    $"Telefono = @Telefono, Poblacion = @Poblacion, Provincia = @Provincia, Codigo_Postal = @Codigo_Postal, NSS = @NSS " +
                    $"WHERE ID_Personal =" + IDPersonal;
                SqlCommand Comando = new SqlCommand(Modificar, Conexion);

                Comando.Parameters.AddWithValue("@ID_Personal", IDPersonal);
                Comando.Parameters.AddWithValue("@Nombre", Nombre);
                Comando.Parameters.AddWithValue("@Direccion", Direccion);
                Comando.Parameters.AddWithValue("@Telefono", Telefono);
                Comando.Parameters.AddWithValue("@Poblacion", Poblacion);
                Comando.Parameters.AddWithValue("@Provincia", Provincia);
                Comando.Parameters.AddWithValue("@Codigo_Postal", CodPostal);
                Comando.Parameters.AddWithValue("@NSS", NSS);

                try
                {
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();

                    Comando.ExecuteNonQuery();
                    FormularioPrincipal.CargarDatos("Personal", this);
                    MessageBox.Show("Registro modificado correctamente");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al modificado el registro: {ex.Message}");
                }
                finally
                {
                    if (Conexion.State == ConnectionState.Open) Conexion.Close();
                }
            }
        }

        private void BtnConsulta_Click(object sender, EventArgs e)
        {
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
        }
    }
}

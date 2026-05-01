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
    public partial class frmMedicos : Form
    {
        Form1 FormularioPrincipal;
        SqlConnection Conexion = new SqlConnection(); 
        public frmMedicos(Form1 Formulario)
        {
            InitializeComponent();
            FormularioPrincipal = Formulario;
            Conexion = FormularioPrincipal.ObtenerConexion();
        }

        private void FormDoctores_Load(object sender, EventArgs e)
        {
            var (btnInsertar, btnEliminar, btnActualizar, btnConsulta, btnLimpiar) =
                FormularioPrincipal.InicializarModulo(this, "Medicos");

            btnInsertar.Click += BtnInsertar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnActualizar.Click += BtnActualizar_Click;
            btnConsulta.Click += BtnConsulta_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
        }

        private (object IDPersonal, object IDMedico, object Cedula) LeerValorCampo()
        {
            return (
                FormularioPrincipal.LeerValorCampo(this, "ID_Personal", "Medicos", FormularioPrincipal.ObtenerTipo("ID_Personal", "Medicos")),
                FormularioPrincipal.LeerValorCampo(this, "ID_Medico", "Medicos", FormularioPrincipal.ObtenerTipo("ID_Medico", "Medicos")),
                FormularioPrincipal.LeerValorCampo(this, "Cedula", "Medicos", FormularioPrincipal.ObtenerTipo("Cedula", "Medicos"))
            );
        }

        private void BtnInsertar_Click(object sender, EventArgs e)
        {
            var (IDPersonal, IDMedico, Cedula) = LeerValorCampo();

            if (IDPersonal == null || Cedula == null)
            {
                MessageBox.Show("Complete todos los campos para continuar");
                return;
            }
            else
            {
                string Agregar = $"INSERT INTO Medicos (ID_Personal, ID_Medico, Cedula) VALUES (@ID_Personal, @ID_Medico, @Cedula)";
                SqlCommand Comando = new SqlCommand(Agregar, Conexion);

                Comando.Parameters.AddWithValue("@ID_Personal", IDPersonal);
                Comando.Parameters.AddWithValue("@ID_Medico", IDMedico);
                Comando.Parameters.AddWithValue("@Cedula", Cedula);

                try
                {
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();

                    Comando.ExecuteNonQuery();
                    FormularioPrincipal.CargarDatos("Medicos", this);
                    MessageBox.Show("Registro insertado correctamente");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Error al insertar: {ex.Message}");
                }
                finally
                {
                    if (Conexion.State == ConnectionState.Open) Conexion.Close();
                }
            }

        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var (_, IDMedico, _) = LeerValorCampo();

            string Eliminar = $"DELETE FROM Medicos WHERE ID_Medico = @ID_Medico";
            SqlCommand Comando = new SqlCommand(Eliminar, Conexion);

            Comando.Parameters.AddWithValue("@ID_Medico", IDMedico);

            try
            {
                if (Conexion.State != ConnectionState.Open) Conexion.Open();

                Comando.ExecuteNonQuery();
                FormularioPrincipal.CargarDatos("Medicos", this);
                MessageBox.Show("Registro elimnado correctamente");
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}");
            }
            finally
            {
                if (Conexion.State == ConnectionState.Open) Conexion.Close();
            }

        }

        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            var (IDPersonal, IDMedico, Cedula) = LeerValorCampo();

            if (IDPersonal == null || Cedula == null)
            {
                MessageBox.Show("Complete todos los campos para continuar");
                return;
            }
            else
            {
                string Agregar = $"UPDATE Medicos SET ID_Personal = @ID";
                SqlCommand Comando = new SqlCommand(Agregar, Conexion);

                Comando.Parameters.AddWithValue("@ID_Personal", IDPersonal);
                Comando.Parameters.AddWithValue("@ID_Medico", IDMedico);
                Comando.Parameters.AddWithValue("@Cedula", Cedula);

                try
                {
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();

                    Comando.ExecuteNonQuery();
                    FormularioPrincipal.CargarDatos("Medicos", this);
                    MessageBox.Show("Registro insertado correctamente");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Error al insertar: {ex.Message}");
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

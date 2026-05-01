using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReVita
{
    public partial class frmVacaciones : Form
    {
        Form1 FormularioPrincipal;
        public frmVacaciones(Form1 Formulario)
        {
            InitializeComponent();
            FormularioPrincipal = Formulario;
        }

        private void frmVacaciones_Load(object sender, EventArgs e)
        {
            var (btnInsertar, btnEliminar, btnActualizar, btnConsulta, btnLimpiar) =
                FormularioPrincipal.InicializarModulo(this, "Vacaciones");

            btnInsertar.Click += BtnInsertar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnActualizar.Click += BtnActualizar_Click;
            btnConsulta.Click += BtnConsulta_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
        }
        private void BtnInsertar_Click(object sender, EventArgs e)
        {
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
        }

        private void BtnActualizar_Click(object sender, EventArgs e)
        {
        }

        private void BtnConsulta_Click(object sender, EventArgs e)
        {
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
        }
    }
}

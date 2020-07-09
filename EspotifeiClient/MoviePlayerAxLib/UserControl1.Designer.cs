namespace MoviePlayerAxLib
{
    partial class UserControl1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserControl1));
            this.axMoviePlayer1 = new AxMOVIEPLAYERLib.AxMoviePlayer();
            ((System.ComponentModel.ISupportInitialize)(this.axMoviePlayer1)).BeginInit();
            this.SuspendLayout();
            // 
            // axMoviePlayer1
            // 
            this.axMoviePlayer1.Enabled = true;
            this.axMoviePlayer1.Location = new System.Drawing.Point(98, 170);
            this.axMoviePlayer1.Name = "axMoviePlayer1";
            this.axMoviePlayer1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axMoviePlayer1.OcxState")));
            this.axMoviePlayer1.Size = new System.Drawing.Size(600, 239);
            this.axMoviePlayer1.TabIndex = 0;
            // 
            // UserControl1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.axMoviePlayer1);
            this.Name = "UserControl1";
            this.Size = new System.Drawing.Size(800, 450);
            ((System.ComponentModel.ISupportInitialize)(this.axMoviePlayer1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxMOVIEPLAYERLib.AxMoviePlayer axMoviePlayer1;
    }
}

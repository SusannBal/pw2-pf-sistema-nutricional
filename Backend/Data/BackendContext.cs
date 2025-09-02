using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class BackendContext : DbContext
    {
        public BackendContext(DbContextOptions<BackendContext> options)
            : base(options)
        {
        }
        // 👇 constructor vacío para que EF pueda instanciarlo en tiempo de diseño
        public BackendContext()
        {
        }
        // 📌 Tablas principales
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Nutricionista> Nutricionistas { get; set; }

        // 📌 Consultas y detalle
        public DbSet<Consulta> Consultas { get; set; }
        public DbSet<Diagnostico> Diagnosticos { get; set; }
        public DbSet<Recomendacion> Recomendaciones { get; set; }

        //Planes y comidas
        public DbSet<PlanNutricional> PlanesNutricionales { get; set; }
        public DbSet<Comida> Comidas { get; set; }

        //Actividades
        public DbSet<ActividadFisica> ActividadesFisicas { get; set; }
        public DbSet<RegistroActividad> RegistrosActividades { get; set; }

        //Historial
        public DbSet<HistorialPaciente> HistorialesPacientes { get; set; }
        public DbSet<HistorialMedico> HistorialesMedicos { get; set; }

        // Recordatorios
        public DbSet<Recordatorio> Recordatorios { get; set; }


        // ✅ Opcional: configuración directa si lo necesitas
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Data Source=SUSANN\\SQLEXPRESS;Initial Catalog=PW2Backend;Integrated Security=True;Trust Server Certificate=True"
                );
            }
        }

    }
}
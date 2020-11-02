namespace BDInSelfLove.Services.Data.Calendar
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Appointment;
    using Microsoft.EntityFrameworkCore;

    public class AppointmentService : IAppointmentService
    {
        private readonly IDeletableEntityRepository<Appointment> appointmentRepository;

        public AppointmentService(IDeletableEntityRepository<Appointment> appointmentRepository)
        {
            this.appointmentRepository = appointmentRepository;
        }

        public IQueryable<AppointmentServiceModel> GetAll(string userId = GlobalConstants.AdministratorRoleName)
        {
            var query = this.appointmentRepository.All().To<AppointmentServiceModel>();

            return userId == GlobalConstants.AdministratorRoleName ? query : query.Where(a => a.UserId == userId);
        }

        public async Task<AppointmentServiceModel> GetById(int id)
        {
            return await this.appointmentRepository.All()
              .To<AppointmentServiceModel>()
              .SingleOrDefaultAsync(appointment => appointment.Id == id);
        }

        public async Task<int> Create(AppointmentServiceModel appointmentServiceModel)
        {
            var appointment = AutoMapperConfig.MapperInstance.Map<Appointment>(appointmentServiceModel);
            appointment.IsApproved = true;

            await this.appointmentRepository.AddAsync(appointment);
            await this.appointmentRepository.SaveChangesAsync();

            return appointment.Id;
        }

        public async Task<AppointmentServiceModel> Delete(int appointmentId)
        {
            var dbAppointment = await this.appointmentRepository.All().FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (dbAppointment == null)
            {
                throw new ArgumentNullException(nameof(dbAppointment));
            }

            this.appointmentRepository.Delete(dbAppointment);
            int result = await this.appointmentRepository.SaveChangesAsync();

            return AutoMapperConfig.MapperInstance.Map<AppointmentServiceModel>(dbAppointment);
        }

        public IQueryable<AppointmentServiceModel> GetAllByDate(DateTime date)
        {
            return this.appointmentRepository.All()
                .Where(a => a.IsApproved &&
                            a.Start.Month == date.Month &&
                            a.Start.Day == date.Day &&
                            a.Start.Year == date.Year)
                .To<AppointmentServiceModel>();
        }
    }
}

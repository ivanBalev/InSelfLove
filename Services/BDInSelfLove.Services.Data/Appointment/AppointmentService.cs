namespace BDInSelfLove.Services.Data.Calendar
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

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

        public IQueryable<AppointmentServiceModel> GetAll(string userId = null)
        {
            var query = this.appointmentRepository.All().To<AppointmentServiceModel>();

            return userId == null ? query : query.Where(a => a.UserId == userId);
        }

        public async Task<AppointmentServiceModel> GetById(int id)
        {
            return await this.appointmentRepository.All()
              .To<AppointmentServiceModel>()
              .SingleOrDefaultAsync(appointment => appointment.Id == id);
        }

        //public async Task<int> Edit(AppointmentServiceModel appointmentServiceModel)
        //{
        //    var dbAppointment = await this.appointmentRepository.All()
        //      .SingleOrDefaultAsync(appointment =>
        //      appointment.Id == appointmentServiceModel.Id);

        //    if (dbAppointment == null)
        //    {
        //        throw new ArgumentNullException(nameof(dbAppointment));
        //    }

        //    dbAppointment.Start = appointmentServiceModel.Start;
        //    dbAppointment.Description = appointmentServiceModel.Description;

        //    this.appointmentRepository.Update(dbAppointment);
        //    int result = await this.appointmentRepository.SaveChangesAsync();

        //    return result;
        //}

        public async Task<int> Create(AppointmentServiceModel appointmentServiceModel)
        {
            var appointment = AutoMapperConfig.MapperInstance.Map<Appointment>(appointmentServiceModel);

            // TODO: Elaborate
            appointment.IsApproved = true;

            await this.appointmentRepository.AddAsync(appointment);
            var result = await this.appointmentRepository.SaveChangesAsync();

            return appointment.Id;
        }

        public async Task<bool> Delete(int appointmentId)
        {
            var dbArticle = await this.appointmentRepository.All().FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (dbArticle == null)
            {
                throw new ArgumentNullException(nameof(dbArticle));
            }

            this.appointmentRepository.Delete(dbArticle);
            int result = await this.appointmentRepository.SaveChangesAsync();

            return result > 0;
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

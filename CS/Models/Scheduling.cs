﻿// Developer Express Code Central Example:
// Scheduler - How to implement a custom Edit Appointment Form with custom fields
// 
// This example illustrates how to implement a custom Appointment Form and display
// it instead of the default one.
// 
// To include a custom Appointment Form to the
// SchedulerPartial view, the
// MVCxSchedulerOptionsForms.SetAppointmentFormTemplateContent Method
// (ms-help://DevExpress.NETv12.2/DevExpress.AspNet/DevExpressWebMvcMVCxSchedulerOptionsForms_SetAppointmentFormTemplateContenttopic.htm)
// should be handled.
// To add custom fields to the Appointment Form, implement a
// custom AppointmentFormTemplateContainer
// (ms-help://DevExpress.NETv12.2/DevExpress.AspNet/clsDevExpressWebASPxSchedulerAppointmentFormTemplateContainertopic.htm)
// and substitute the default container with your custom one by handling the
// ASPxScheduler.AppointmentFormShowing Event
// (ms-help://DevExpress.NETv12.2/DevExpress.AspNet/DevExpressWebASPxSchedulerASPxScheduler_AppointmentFormShowingtopic.htm).
// See
// Also:
// http://www.devexpress.com/scid=E2924
// http://www.devexpress.com/scid=E3984
// 
// You can find sample updates and versions for different programming languages here:
// http://www.devexpress.com/example=E4520

using System.Collections;
using System.Linq;
using DevExpressMvcApplication1;
using DevExpress.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System;
using DevExpressMvcApplication1.Models;
using DevExpress.Web;
using DevExpress.Web.ASPxScheduler;
using DevExpress.XtraScheduler;

public class SchedulerDataObject {
    public IEnumerable Appointments { get; set; }
    public IEnumerable Resources { get; set; }
}

public class SchedulerDataHelper {
    public static IEnumerable GetResources() {
        CarsDataContext db = new CarsDataContext();
        return from res in db.Cars select res;
    }
    public static IEnumerable GetAppointments() {
        CarsDataContext db = new CarsDataContext();
        return from apt in db.CarSchedulings select apt;
    }
    public static IEnumerable GetReminders(IEnumerable rawDataSource) {
        foreach (ListEditItem item in rawDataSource) {
            yield return new { Value = item.Value, Text = item.Text };
        }
    }
    public static SchedulerDataObject DataObject {
        get {
            return new SchedulerDataObject() {
                Appointments = GetAppointments(),
                Resources = GetResources()
            };
        }
    }
    static MVCxAppointmentStorage defaultAppointmentStorage;
    public static MVCxAppointmentStorage DefaultAppointmentStorage {
        get {
            if (defaultAppointmentStorage == null)
                defaultAppointmentStorage = CreateDefaultAppointmentStorage();
            return defaultAppointmentStorage;
        }
    }
    static MVCxAppointmentStorage CreateDefaultAppointmentStorage() {
        MVCxAppointmentStorage appointmentStorage = new MVCxAppointmentStorage();
        appointmentStorage.Mappings.AppointmentId = "ID";
        appointmentStorage.Mappings.Start = "StartTime";
        appointmentStorage.Mappings.End = "EndTime";
        appointmentStorage.Mappings.Subject = "Subject";
        appointmentStorage.Mappings.Description = "Description";
        appointmentStorage.Mappings.Location = "Location";
        appointmentStorage.Mappings.AllDay = "AllDay";
        appointmentStorage.Mappings.Type = "EventType";
        appointmentStorage.Mappings.RecurrenceInfo = "RecurrenceInfo";
        appointmentStorage.Mappings.ReminderInfo = "ReminderInfo";
        appointmentStorage.Mappings.Label = "Label";
        appointmentStorage.Mappings.Status = "Status";
        appointmentStorage.Mappings.ResourceId = "CarId";
        appointmentStorage.CustomFieldMappings.Add("Price", "Price");
        appointmentStorage.CustomFieldMappings.Add("ContactInfo", "ContactInfo");
        appointmentStorage.CustomFieldMappings.Add("CarId", "CarId");
        return appointmentStorage;
    }
    static MVCxResourceStorage defaultResourceStorage;
    public static MVCxResourceStorage DefaultResourceStorage {
        get {
            if (defaultResourceStorage == null)
                defaultResourceStorage = CreateDefaultResourceStorage();
            return defaultResourceStorage;
        }
    }
    static MVCxResourceStorage CreateDefaultResourceStorage() {
        MVCxResourceStorage resourceStorage = new MVCxResourceStorage();
        resourceStorage.Mappings.ResourceId = "ID";
        resourceStorage.Mappings.Caption = "Model";
        return resourceStorage;
    }
    public static void InsertAppointment(CarScheduling appt) {
        if (appt == null)
            return;
        CarsDataContext db = new CarsDataContext();
        appt.ID = appt.GetHashCode();
        db.CarSchedulings.InsertOnSubmit(appt);
        db.SubmitChanges();
    }
    public static void UpdateAppointment(CarScheduling appt) {
        if (appt == null)
            return;
        CarsDataContext db = new CarsDataContext();
        CarScheduling query = (CarScheduling)(from carSchedule in db.CarSchedulings where carSchedule.ID == appt.ID select carSchedule).SingleOrDefault();

        query.ID = appt.ID;
        query.StartTime = appt.StartTime;
        query.EndTime = appt.EndTime;
        query.AllDay = appt.AllDay;
        query.Subject = appt.Subject;
        query.Description = appt.Description;
        query.Location = appt.Location;
        query.RecurrenceInfo = appt.RecurrenceInfo;
        query.ReminderInfo = appt.ReminderInfo;
        query.Status = appt.Status;
        query.EventType = appt.EventType;
        query.Label = appt.Label;
        query.CarId = appt.CarId;
        query.ContactInfo = appt.ContactInfo;
        query.Price = appt.Price;
        db.SubmitChanges();
    }
    public static void RemoveAppointment(CarScheduling appt) {
        CarsDataContext db = new CarsDataContext();
        CarScheduling query = (CarScheduling)(from carSchedule in db.CarSchedulings where carSchedule.ID == appt.ID select carSchedule).SingleOrDefault();
        db.CarSchedulings.DeleteOnSubmit(query);
        db.SubmitChanges();
    }
}

public class CustomAppointmentTemplateContainer : AppointmentFormTemplateContainer {
    public CustomAppointmentTemplateContainer(MVCxScheduler scheduler)
        : base(scheduler) {
    }

    public new IEnumerable ResourceDataSource {
        get { return SchedulerDataHelper.GetResources(); }
    }
    public new IEnumerable ReminderDataSource {
        get { return SchedulerDataHelper.GetReminders(base.ReminderDataSource); }
    }
    public string ContactInfo {
        get { return Convert.ToString(Appointment.CustomFields["ContactInfo"]); }
    }
    public decimal? Price {
        get {
            object priceRawValue = Appointment.CustomFields["Price"];
            return priceRawValue == DBNull.Value ? 0 : (decimal?)priceRawValue;
        }
    }
    public int? CarId {
        get {
            object carId = Appointment.ResourceId;
            return carId == ResourceEmpty.Id ? 1 : (int?)carId; // select first resource if empty
        }
    }
}

public class Schedule {
    public Schedule() {
    }
    public Schedule(CarScheduling carScheduling) {
        if (carScheduling != null) {
            ID = carScheduling.ID;
            EventType = carScheduling.EventType;
            Label = carScheduling.Label;
            AllDay = carScheduling.AllDay;
            Location = carScheduling.Location;
            CarId = carScheduling.CarId;
            Status = carScheduling.Status;
            RecurrenceInfo = carScheduling.RecurrenceInfo;
            ReminderInfo = carScheduling.ReminderInfo;
            Subject = carScheduling.Subject;
            Price = carScheduling.Price;
            StartTime = carScheduling.StartTime.Value;
            EndTime = carScheduling.EndTime.Value;
            Description = carScheduling.Description;
            ContactInfo = carScheduling.ContactInfo;
        }
    }

    public int ID { get; set; }
    public int? EventType { get; set; }
    public int? Label { get; set; }
    public bool AllDay { get; set; }
    public string Location { get; set; }
    public object CarId { get; set; }
    public int? Status { get; set; }
    public string RecurrenceInfo { get; set; }
    public string ReminderInfo { get; set; }
    [Required(ErrorMessage = "The Subject must contain at least one character.")]
    public string Subject { get; set; }
    public decimal? Price { get; set; }
    [Required]
    public DateTime StartTime { get; set; }
    [Required]
    public DateTime EndTime { get; set; }
    public string Description { get; set; }
    public string ContactInfo { get; set; }
    public bool HasReminder { get; set; }
    public Reminder Reminder { get; set; }

    public virtual void Assign(Schedule source) {
        if (source != null) {
            ID = source.ID;
            EventType = source.EventType;
            Label = source.Label;
            AllDay = source.AllDay;
            Location = source.Location;
            CarId = source.CarId;
            Status = source.Status;
            RecurrenceInfo = source.RecurrenceInfo;
            ReminderInfo = source.ReminderInfo;
            Subject = source.Subject;
            Price = source.Price;
            StartTime = source.StartTime;
            EndTime = source.EndTime;
            Description = source.Description;
            ContactInfo = source.ContactInfo;
        }
    }
}
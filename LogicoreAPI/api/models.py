from django.db import models
from django.db import models

# 1️⃣ ROLES
class Roles(models.Model):
    role_id = models.IntegerField(primary_key=True)
    role_name = models.CharField(max_length=50)
    description = models.CharField(max_length=200)

    def __str__(self):
        return self.role_name


# 2️⃣ USERS
class Users(models.Model):
    user_id = models.IntegerField(primary_key=True)
    role_id = models.IntegerField()
    full_name = models.CharField(max_length=100)
    email = models.CharField(max_length=100, unique=True)
    password_hash = models.CharField(max_length=255)
    status = models.CharField(max_length=20)
    created_at = models.DateField()

    def __str__(self):
        return self.full_name


# 3️⃣ VEHICLES
class Vehicles(models.Model):
    vehicle_id = models.IntegerField(primary_key=True)
    vehicle_name = models.CharField(max_length=100)
    vehicle_type = models.CharField(max_length=50)
    license_plate = models.CharField(max_length=20, unique=True)
    max_capacity_kg = models.IntegerField()
    odometer = models.IntegerField()
    acquisition_cost = models.IntegerField()
    status = models.CharField(max_length=30)
    created_at = models.DateField()

    def __str__(self):
        return self.vehicle_name


# 4️⃣ DRIVERS
class Drivers(models.Model):
    driver_id = models.IntegerField(primary_key=True)
    driver_name = models.CharField(max_length=100)
    license_number = models.CharField(max_length=50, unique=True)
    license_category = models.CharField(max_length=30)
    license_expiry = models.DateField()
    safety_score = models.IntegerField()
    status = models.CharField(max_length=30)
    created_at = models.DateField()

    def __str__(self):
        return self.driver_name


# 5️⃣ TRIPS
class Trips(models.Model):
    trip_id = models.IntegerField(primary_key=True)
    vehicle_id = models.IntegerField()
    driver_id = models.IntegerField()
    cargo_weight = models.IntegerField()
    origin = models.CharField(max_length=100)
    destination = models.CharField(max_length=100)
    start_date = models.DateField()
    end_date = models.DateField()
    start_odometer = models.IntegerField()
    end_odometer = models.IntegerField()
    status = models.CharField(max_length=30)
    created_at = models.DateField()

    def __str__(self):
        return str(self.trip_id)


# 6️⃣ MAINTENANCE_LOGS
class MaintenanceLogs(models.Model):
    maintenance_id = models.IntegerField(primary_key=True)
    vehicle_id = models.IntegerField()
    service_type = models.CharField(max_length=100)
    service_date = models.DateField()
    next_service_due = models.DateField()
    cost = models.IntegerField()
    remarks = models.CharField(max_length=255)

    def __str__(self):
        return str(self.maintenance_id)


# 7️⃣ FUEL_LOGS
class FuelLogs(models.Model):
    fuel_id = models.IntegerField(primary_key=True)
    vehicle_id = models.IntegerField()
    trip_id = models.IntegerField()
    liters = models.IntegerField()
    cost = models.IntegerField()
    fuel_date = models.DateField()

    def __str__(self):
        return str(self.fuel_id)


# 8️⃣ EXPENSES
class Expenses(models.Model):
    expense_id = models.IntegerField(primary_key=True)
    vehicle_id = models.IntegerField()
    expense_type = models.CharField(max_length=100)
    amount = models.IntegerField()
    expense_date = models.DateField()
    remarks = models.CharField(max_length=255)

    def __str__(self):
        return str(self.expense_id)


# 9️⃣ REVENUE
class Revenue(models.Model):
    revenue_id = models.IntegerField(primary_key=True)
    trip_id = models.IntegerField()
    revenue_amount = models.IntegerField()
    received_date = models.DateField()

    def __str__(self):
        return str(self.revenue_id)
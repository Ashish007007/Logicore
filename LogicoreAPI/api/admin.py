from django.contrib import admin
from .models import Roles, Users, Vehicles, Drivers, Trips, MaintenanceLogs, FuelLogs, Expenses, Revenue

admin.site.register(Roles)
admin.site.register(Users)
admin.site.register(Vehicles)
admin.site.register(Drivers)
admin.site.register(Trips)
admin.site.register(MaintenanceLogs)
admin.site.register(FuelLogs)
admin.site.register(Expenses)
admin.site.register(Revenue)


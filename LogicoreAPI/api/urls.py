"""
URL configuration for logicore project.

The `urlpatterns` list routes URLs to views. For more information please see:
    https://docs.djangoproject.com/en/6.0/topics/http/urls/
Examples:
Function views
    1. Add an import:  from my_app import views
    2. Add a URL to urlpatterns:  path('', views.home, name='home')
Class-based views
    1. Add an import:  from other_app.views import Home
    2. Add a URL to urlpatterns:  path('', Home.as_view(), name='home')
Including another URLconf
    1. Import the include() function: from django.urls import include, path
    2. Add a URL to urlpatterns:  path('blog/', include('blog.urls'))
"""
from django.contrib import admin
from django.urls import path
from .views import string_api,Sample
from api.views import home
from .import views
urlpatterns = [
    path("string-post/", string_api),
    path('sample/', Sample, name="Sample"),
    path('register/', views.register_user),
    path('login/', views.login_user),
    path('get-user/', views.get_user_by_id),
    path('roles/create/', views.create_role),
    path('roles/all/', views.get_all_roles),
    path('roles/get/', views.get_role_by_id),
    path('roles/update/', views.update_role),
    path('roles/delete/', views.delete_role),
    path('vehicles/create/', views.create_vehicle),
    path('vehicles/all/', views.get_all_vehicles),
    path('vehicles/get/', views.get_vehicle_by_id),
    path('vehicles/update/', views.update_vehicle),
    path('vehicles/delete/', views.delete_vehicle),
    path('vehicles/deactivate/', views.deactivate_vehicle),
    path('drivers/create/', views.create_driver),
    path('drivers/all/', views.get_all_drivers),
    path('drivers/get/', views.get_driver_by_id),
    path('drivers/update/', views.update_driver),
    path('drivers/deactivate/', views.deactivate_driver),
    path('trips/create/', views.create_trip),
    path('trips/all/', views.get_all_trips),
    path('trips/get/', views.get_trip_by_id),
    path('trips/complete/', views.complete_trip),
    path('maintenance/create/', views.create_maintenance_log),
    path('maintenance/all/', views.get_all_maintenance_logs),
    path('maintenance/by-vehicle/', views.get_maintenance_by_vehicle),
    path('maintenance/complete/', views.complete_maintenance),
    path('fuel/create/', views.create_fuel_log),
    path('fuel/all/', views.get_all_fuel_logs),
    path('fuel/by-vehicle/', views.get_fuel_by_vehicle),
    path('fuel/efficiency/', views.calculate_fuel_efficiency),
    path('expenses/create/', views.create_expense),
    path('expenses/all/', views.get_all_expenses),
    path('expenses/by-vehicle/', views.get_expense_by_vehicle),
    path('expenses/total/', views.calculate_total_expense),
    path('revenue/create/', views.create_revenue),
    path('revenue/all/', views.get_all_revenue),
    path('revenue/by-trip/', views.get_revenue_by_trip),
    path('revenue/roi/', views.calculate_roi),
            

]

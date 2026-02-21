from django.shortcuts import render
from rest_framework.decorators import api_view
from rest_framework.response import Response
import json
from django.http import JsonResponse
from django.views.decorators.csrf import csrf_exempt
from .models import Drivers, Users, Roles,Vehicles,Trips,MaintenanceLogs,FuelLogs,Expenses,Revenue
from datetime import date,datetime


@api_view(['POST'])
def Sample(request):
    text = request.data.get("text")
    return Response(text)
    
@api_view(['POST'])
def string_api(request):

    text = request.data.get("text")

    if not text:
        return Response({"error": "text required"}, status=400)

    data = {
        "message": "success",
        "input": text,
        "length": len(text),
        "upper": text.upper(),
        "lower": text.lower()
    }

    return Response(data)

@api_view(['GET'])
def home(request):
    return Response({"message": "API working"})

@csrf_exempt
def register_user(request):
    if request.method == "POST":
        try:    
            data = json.loads(request.body)

            role_id = data.get("role_id")
            full_name = data.get("full_name")
            email = data.get("email")
            password = data.get("password")
            status = data.get("status")

            # Validation
            if not all([role_id, full_name, email, password, status]):
                return JsonResponse({"error": "All fields are required"}, status=400)

            # Email already exists check
            if Users.objects.filter(email=email).exists():
                return JsonResponse({"error": "Email already registered"}, status=400)

            # Role exists check
            if not Roles.objects.filter(role_id=role_id).exists():
                return JsonResponse({"error": "Invalid role_id"}, status=400)

            # Create User
            user = Users.objects.create(
                user_id=Users.objects.count() + 1,
                role_id=role_id,
                full_name=full_name,
                email=email,
                password_hash=password,
                status=status,
                created_at=date.today()
            )

            return JsonResponse({
                "message": "User registered successfully",
                "user_id": user.user_id
            })

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405)


@api_view(["POST"])
def login_user(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)

            email = data.get("email")
            password = data.get("password")

            if not all([email, password]):
                return JsonResponse({"error": "Email and Password required"}, status=400)

            user = Users.objects.filter(email=email, password_hash=password).first()

            if not user:
                return JsonResponse({"error": "Invalid credentials"}, status=401)

            if user.status != "Active":
                return JsonResponse({"error": "User not active"}, status=403)

            user = Users.objects.filter(user_id=user.user_id).first()

            if not user:
                return JsonResponse({"error": "User not found"}, status=404)

            return JsonResponse({
                "user_id": user.user_id,
                "role_id": user.role_id,
                "full_name": user.full_name,
                "email": user.email,
                "status": user.status,
                "created_at": user.created_at
            })

            return get_user_by_id(request)

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405)

#  #   
#  USER ##############################################################  
@csrf_exempt
def get_user_by_id(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)
            user_id = data.get("user_id")

            if not user_id:
                return JsonResponse({"error": "user_id is required"}, status=400)

            user = Users.objects.filter(user_id=user_id).first()

            if not user:
                return JsonResponse({"error": "User not found"}, status=404)

            return JsonResponse({
                "user_id": user.user_id,
                "role_id": user.role_id,
                "full_name": user.full_name,
                "email": user.email,
                "status": user.status,
                "created_at": user.created_at
            })

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405)

# 1️⃣ Create Role
@csrf_exempt
def create_role(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)

            role_id = data.get("role_id")
            role_name = data.get("role_name")
            description = data.get("description")

            if not all([role_id, role_name, description]):
                return JsonResponse({"error": "All fields are required"}, status=400)

            if Roles.objects.filter(role_id=role_id).exists():
                return JsonResponse({"error": "Role ID already exists"}, status=400)

            role = Roles.objects.create(
                role_id=role_id,
                role_name=role_name,
                description=description
            )

            return JsonResponse({
                "message": "Role created successfully",
                "role_id": role.role_id
            })

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405)


# 2️⃣ Get All Roles
def get_all_roles(request):
    roles = Roles.objects.all()
    data = []

    for role in roles:
        data.append({
            "role_id": role.role_id,
            "role_name": role.role_name,
            "description": role.description
        })

    return JsonResponse({"roles": data})


# 3️⃣ Get Role By ID
@csrf_exempt
def get_role_by_id(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)
            role_id = data.get("role_id")

            role = Roles.objects.filter(role_id=role_id).first()

            if not role:
                return JsonResponse({"error": "Role not found"}, status=404)

            return JsonResponse({
                "role_id": role.role_id,
                "role_name": role.role_name,
                "description": role.description
            })

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405)


# 4️⃣ Update Role
@csrf_exempt
def update_role(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)
            role_id = data.get("role_id")

            role = Roles.objects.filter(role_id=role_id).first()

            if not role:
                return JsonResponse({"error": "Role not found"}, status=404)

            role.role_name = data.get("role_name", role.role_name)
            role.description = data.get("description", role.description)
            role.save()

            return JsonResponse({"message": "Role updated successfully"})

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405)


# 5️⃣ Delete Role
@csrf_exempt
def delete_role(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)
            role_id = data.get("role_id")

            role = Roles.objects.filter(role_id=role_id).first()

            if not role:
                return JsonResponse({"error": "Role not found"}, status=404)

            role.delete()

            return JsonResponse({"message": "Role deleted successfully"})

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405)

# 1️⃣ Create Vehicle
@csrf_exempt
def create_vehicle(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)

            vehicle_id = data.get("vehicle_id")
            vehicle_name = data.get("vehicle_name")
            vehicle_type = data.get("vehicle_type")
            license_plate = data.get("license_plate")
            max_capacity_kg = data.get("max_capacity_kg")
            odometer = data.get("odometer")
            acquisition_cost = data.get("acquisition_cost")
            status = data.get("status")

            if not all([vehicle_id, vehicle_name, vehicle_type, license_plate,
                        max_capacity_kg, odometer, acquisition_cost, status]):
                return JsonResponse({"error": "All fields are required"}, status=400)

            if Vehicles.objects.filter(vehicle_id=vehicle_id).exists():
                return JsonResponse({"error": "Vehicle ID already exists"}, status=400)

            if Vehicles.objects.filter(license_plate=license_plate).exists():
                return JsonResponse({"error": "License plate already exists"}, status=400)

            vehicle = Vehicles.objects.create(
                vehicle_id=vehicle_id,
                vehicle_name=vehicle_name,
                vehicle_type=vehicle_type,
                license_plate=license_plate,
                max_capacity_kg=max_capacity_kg,
                odometer=odometer,
                acquisition_cost=acquisition_cost,
                status=status,
                created_at=date.today()
            )

            return JsonResponse({
                "message": "Vehicle created successfully",
                "vehicle_id": vehicle.vehicle_id
            })

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405)


# 2️⃣ Get All Vehicles
def get_all_vehicles(request):
    vehicles = Vehicles.objects.all()
    data = []

    for v in vehicles:
        data.append({
            "vehicle_id": v.vehicle_id,
            "vehicle_name": v.vehicle_name,
            "vehicle_type": v.vehicle_type,
            "license_plate": v.license_plate,
            "max_capacity_kg": v.max_capacity_kg,
            "odometer": v.odometer,
            "acquisition_cost": v.acquisition_cost,
            "status": v.status,
            "created_at": v.created_at
        })

    return JsonResponse({"vehicles": data})


# 3️⃣ Get Vehicle By ID
@csrf_exempt
def get_vehicle_by_id(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)
            vehicle_id = data.get("vehicle_id")

            vehicle = Vehicles.objects.filter(vehicle_id=vehicle_id).first()

            if not vehicle:
                return JsonResponse({"error": "Vehicle not found"}, status=404)

            return JsonResponse({
                "vehicle_id": vehicle.vehicle_id,
                "vehicle_name": vehicle.vehicle_name,
                "vehicle_type": vehicle.vehicle_type,
                "license_plate": vehicle.license_plate,
                "max_capacity_kg": vehicle.max_capacity_kg,
                "odometer": vehicle.odometer,
                "acquisition_cost": vehicle.acquisition_cost,
                "status": vehicle.status,
                "created_at": vehicle.created_at
            })

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405)


# 4️⃣ Update Vehicle
@csrf_exempt
def update_vehicle(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)
            vehicle_id = data.get("vehicle_id")

            vehicle = Vehicles.objects.filter(vehicle_id=vehicle_id).first()

            if not vehicle:
                return JsonResponse({"error": "Vehicle not found"}, status=404)

            vehicle.vehicle_name = data.get("vehicle_name", vehicle.vehicle_name)
            vehicle.vehicle_type = data.get("vehicle_type", vehicle.vehicle_type)
            vehicle.license_plate = data.get("license_plate", vehicle.license_plate)
            vehicle.max_capacity_kg = data.get("max_capacity_kg", vehicle.max_capacity_kg)
            vehicle.odometer = data.get("odometer", vehicle.odometer)
            vehicle.acquisition_cost = data.get("acquisition_cost", vehicle.acquisition_cost)
            vehicle.status = data.get("status", vehicle.status)
            vehicle.save()

            return JsonResponse({"message": "Vehicle updated successfully"})

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405)


# 5️⃣ Delete Vehicle
@csrf_exempt
def delete_vehicle(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)
            vehicle_id = data.get("vehicle_id")

            vehicle = Vehicles.objects.filter(vehicle_id=vehicle_id).first()

            if not vehicle:
                return JsonResponse({"error": "Vehicle not found"}, status=404)

            vehicle.delete()

            return JsonResponse({"message": "Vehicle deleted successfully"})

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405)

@csrf_exempt
def deactivate_vehicle(request):
    if request.method == "POST":
        data = json.loads(request.body)

        vehicle = Vehicles.objects.filter(vehicle_id=data.get("vehicle_id")).first()

        if not vehicle:
            return JsonResponse({"error": "Vehicle not found"}, status=404)

        vehicle.status = "Inactive"
        vehicle.save()

        return JsonResponse({"message": "Vehicle deactivated successfully"})

    return JsonResponse({"error": "Invalid request"}, status=405)

######################### DRIVER #####

@csrf_exempt
def create_driver(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)

            driver_id = data.get("driver_id")
            driver_name = data.get("driver_name")
            license_number = data.get("license_number")
            license_category = data.get("license_category")
            license_expiry = data.get("license_expiry")
            safety_score = data.get("safety_score")
            status = data.get("status")

            # Required fields check
            if not all([driver_id, driver_name, license_number,
                        license_category, license_expiry,
                        safety_score, status]):
                return JsonResponse({"error": "All fields are required"}, status=400)

            # Duplicate checks
            if Drivers.objects.filter(driver_id=driver_id).exists():
                return JsonResponse({"error": "Driver ID already exists"}, status=400)

            if Drivers.objects.filter(license_number=license_number).exists():
                return JsonResponse({"error": "License number already exists"}, status=400)

            # Safety score validation
            if int(safety_score) < 0 or int(safety_score) > 100:
                return JsonResponse({"error": "Safety score must be between 0 and 100"}, status=400)

            # License expiry validation
            expiry_date = datetime.strptime(license_expiry, "%Y-%m-%d").date()
            if expiry_date <= date.today():
                return JsonResponse({"error": "License expiry must be future date"}, status=400)

            driver = Drivers.objects.create(
                driver_id=driver_id,
                driver_name=driver_name,
                license_number=license_number,
                license_category=license_category,
                license_expiry=expiry_date,
                safety_score=safety_score,
                status=status,
                created_at=date.today()
            )

            return JsonResponse({
                "message": "Driver created successfully",
                "driver_id": driver.driver_id
            })

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request"}, status=405)

def get_all_drivers(request):
    drivers = Drivers.objects.all()
    data = []

    for d in drivers:
        data.append({
            "driver_id": d.driver_id,
            "driver_name": d.driver_name,
            "license_number": d.license_number,
            "license_category": d.license_category,
            "license_expiry": d.license_expiry,
            "safety_score": d.safety_score,
            "status": d.status,
            "created_at": d.created_at
        })

    return JsonResponse({"drivers": data})

@csrf_exempt
def get_driver_by_id(request):
    if request.method == "POST":
        data = json.loads(request.body)
        driver = Drivers.objects.filter(driver_id=data.get("driver_id")).first()

        if not driver:
            return JsonResponse({"error": "Driver not found"}, status=404)

        return JsonResponse({
            "driver_id": driver.driver_id,
            "driver_name": driver.driver_name,
            "license_number": driver.license_number,
            "license_category": driver.license_category,
            "license_expiry": driver.license_expiry,
            "safety_score": driver.safety_score,
            "status": driver.status,
            "created_at": driver.created_at
        })

    return JsonResponse({"error": "Invalid request"}, status=405)

@csrf_exempt
def update_driver(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)
            driver = Drivers.objects.filter(driver_id=data.get("driver_id")).first()

            if not driver:
                return JsonResponse({"error": "Driver not found"}, status=404)

            # Update fields if provided
            driver.driver_name = data.get("driver_name", driver.driver_name)
            driver.license_category = data.get("license_category", driver.license_category)
            driver.status = data.get("status", driver.status)

            if data.get("safety_score"):
                score = int(data.get("safety_score"))
                if score < 0 or score > 100:
                    return JsonResponse({"error": "Safety score must be between 0 and 100"}, status=400)
                driver.safety_score = score

            driver.save()

            return JsonResponse({"message": "Driver updated successfully"})

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request"}, status=405)

@csrf_exempt
def deactivate_driver(request):
    if request.method == "POST":
        data = json.loads(request.body)
        driver = Drivers.objects.filter(driver_id=data.get("driver_id")).first()

        if not driver:
            return JsonResponse({"error": "Driver not found"}, status=404)

        driver.status = "Inactive"
        driver.save()

        return JsonResponse({"message": "Driver deactivated successfully"})

    return JsonResponse({"error": "Invalid request"}, status=405)
###### tripppppppppppppppppppp################################
@csrf_exempt
def create_trip(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)

            trip_id = data.get("trip_id")
            vehicle_id = data.get("vehicle_id")
            driver_id = data.get("driver_id")
            cargo_weight = data.get("cargo_weight")
            origin = data.get("origin")
            destination = data.get("destination")
            start_date = data.get("start_date")
            start_odometer = data.get("start_odometer")

            # ✅ Required field check
            if not all([trip_id, vehicle_id, driver_id, cargo_weight,
                        origin, destination, start_date, start_odometer]):
                return JsonResponse({"error": "All required fields are mandatory"}, status=400)

            # ✅ Trip duplicate check
            if Trips.objects.filter(trip_id=trip_id).exists():
                return JsonResponse({"error": "Trip ID already exists"}, status=400)

            # ✅ Vehicle check
            vehicle = Vehicles.objects.filter(vehicle_id=vehicle_id).first()
            if not vehicle:
                return JsonResponse({"error": "Vehicle not found"}, status=404)

            if vehicle.status != "Available":
                return JsonResponse({"error": "Vehicle is not available"}, status=400)

            # ✅ Driver check
            driver = Drivers.objects.filter(driver_id=driver_id).first()
            if not driver:
                return JsonResponse({"error": "Driver not found"}, status=404)

            if driver.status != "On Duty":
                return JsonResponse({"error": "Driver is not available"}, status=400)

            # ✅ Capacity validation
            if int(cargo_weight) > int(vehicle.max_capacity_kg):
                return JsonResponse({"error": "Cargo exceeds vehicle capacity"}, status=400)

            # ✅ License expiry validation
            if driver.license_expiry <= date.today():
                return JsonResponse({"error": "Driver license expired"}, status=400)

            # ✅ Date convert
            start_date_obj = datetime.strptime(start_date, "%Y-%m-%d").date()

            # ✅ Create trip
            trip = Trips.objects.create(
                trip_id=trip_id,
                vehicle_id=vehicle_id,
                driver_id=driver_id,
                cargo_weight=cargo_weight,
                origin=origin,
                destination=destination,
                start_date=start_date_obj,
                start_odometer=start_odometer,
                status="Dispatched",
                created_at=date.today()
            )

            # ✅ Update Vehicle & Driver Status
            vehicle.status = "On Trip"
            vehicle.save()

            driver.status = "On Trip"
            driver.save()

            return JsonResponse({
                "message": "Trip created successfully",
                "trip_id": trip.trip_id
            })

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405)

def get_all_trips(request):
    trips = Trips.objects.all()
    data = []

    for t in trips:
        data.append({
            "trip_id": t.trip_id,
            "vehicle_id": t.vehicle_id,
            "driver_id": t.driver_id,
            "cargo_weight": t.cargo_weight,
            "origin": t.origin,
            "destination": t.destination,
            "start_date": t.start_date,
            "end_date": t.end_date,
            "status": t.status
        })

    return JsonResponse({"trips": data})

@csrf_exempt
def get_trip_by_id(request):
    if request.method == "POST":
        data = json.loads(request.body)
        trip = Trips.objects.filter(trip_id=data.get("trip_id")).first()

        if not trip:
            return JsonResponse({"error": "Trip not found"}, status=404)

        return JsonResponse({
            "trip_id": trip.trip_id,
            "vehicle_id": trip.vehicle_id,
            "driver_id": trip.driver_id,
            "cargo_weight": trip.cargo_weight,
            "origin": trip.origin,
            "destination": trip.destination,
            "start_date": trip.start_date,
            "end_date": trip.end_date,
            "status": trip.status
        })

    return JsonResponse({"error": "Invalid request method"}, status=405)

@csrf_exempt
def complete_trip(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)
            trip = Trips.objects.filter(trip_id=data.get("trip_id")).first()

            if not trip:
                return JsonResponse({"error": "Trip not found"}, status=404)

            end_odometer = data.get("end_odometer")
            if not end_odometer:
                return JsonResponse({"error": "End odometer required"}, status=400)

            trip.end_odometer = end_odometer
            trip.end_date = date.today()
            trip.status = "Completed"
            trip.save()

            # Reset Vehicle
            vehicle = Vehicles.objects.get(vehicle_id=trip.vehicle_id)
            vehicle.status = "Available"
            vehicle.odometer = end_odometer
            vehicle.save()

            # Reset Driver
            driver = Drivers.objects.get(driver_id=trip.driver_id)
            driver.status = "On Duty"
            driver.save()

            return JsonResponse({"message": "Trip completed successfully"})

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405)

## Maintainance log ##############################

@csrf_exempt
def create_maintenance_log(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)

            maintenance_id = data.get("maintenance_id")
            vehicle_id = data.get("vehicle_id")
            service_type = data.get("service_type")
            service_date = data.get("service_date")
            next_service_due = data.get("next_service_due")
            cost = data.get("cost")
            remarks = data.get("remarks")

            # Required validation
            if not all([maintenance_id, vehicle_id, service_type,
                        service_date, next_service_due, cost]):
                return JsonResponse({"error": "All required fields are mandatory"}, status=400)

            # Duplicate check
            if MaintenanceLogs.objects.filter(maintenance_id=maintenance_id).exists():
                return JsonResponse({"error": "Maintenance ID already exists"}, status=400)

            vehicle = Vehicles.objects.filter(vehicle_id=vehicle_id).first()
            if not vehicle:
                return JsonResponse({"error": "Vehicle not found"}, status=404)

            # Date validation
            service_date_obj = datetime.strptime(service_date, "%Y-%m-%d").date()
            next_due_obj = datetime.strptime(next_service_due, "%Y-%m-%d").date()

            if next_due_obj <= service_date_obj:
                return JsonResponse({"error": "Next service due must be after service date"}, status=400)

            # Cost validation
            if float(cost) < 0:
                return JsonResponse({"error": "Cost cannot be negative"}, status=400)

            # Create log
            log = MaintenanceLogs.objects.create(
                maintenance_id=maintenance_id,
                vehicle_id=vehicle_id,
                service_type=service_type,
                service_date=service_date_obj,
                next_service_due=next_due_obj,
                cost=cost,
                remarks=remarks
            )

            # Auto update vehicle status
            vehicle.status = "In Shop"
            vehicle.save()

            return JsonResponse({
                "message": "Maintenance log created successfully",
                "maintenance_id": log.maintenance_id
            })

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405)
def get_all_maintenance_logs(request):
    logs = MaintenanceLogs.objects.all()
    data = []

    for l in logs:
        data.append({
            "maintenance_id": l.maintenance_id,
            "vehicle_id": l.vehicle_id,
            "service_type": l.service_type,
            "service_date": l.service_date,
            "next_service_due": l.next_service_due,
            "cost": l.cost,
            "remarks": l.remarks
        })

    return JsonResponse({"maintenance_logs": data})

@csrf_exempt
def get_maintenance_by_vehicle(request):
    if request.method == "POST":
        data = json.loads(request.body)
        vehicle_id = data.get("vehicle_id")

        logs = MaintenanceLogs.objects.filter(vehicle_id=vehicle_id)

        if not logs:
            return JsonResponse({"error": "No maintenance logs found"}, status=404)

        data_list = []
        for l in logs:
            data_list.append({
                "maintenance_id": l.maintenance_id,
                "service_type": l.service_type,
                "service_date": l.service_date,
                "next_service_due": l.next_service_due,
                "cost": l.cost,
                "remarks": l.remarks
            })

        return JsonResponse({"maintenance_logs": data_list})

    return JsonResponse({"error": "Invalid request method"}, status=405)

@csrf_exempt
def complete_maintenance(request):
    if request.method == "POST":
        data = json.loads(request.body)
        vehicle = Vehicles.objects.filter(vehicle_id=data.get("vehicle_id")).first()

        if not vehicle:
            return JsonResponse({"error": "Vehicle not found"}, status=404)

        vehicle.status = "Available"
        vehicle.save()

        return JsonResponse({"message": "Vehicle marked as Available"})

    return JsonResponse({"error": "Invalid request method"}, status=405)


## Fuel Logs ################################

@csrf_exempt
def create_fuel_log(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)

            fuel_id = data.get("fuel_id")
            vehicle_id = data.get("vehicle_id")
            trip_id = data.get("trip_id")
            liters = data.get("liters")
            cost = data.get("cost")
            fuel_date = data.get("fuel_date")

            # Required validation
            if not all([fuel_id, vehicle_id, trip_id, liters, cost, fuel_date]):
                return JsonResponse({"error": "All required fields are mandatory"}, status=400)

            if FuelLogs.objects.filter(fuel_id=fuel_id).exists():
                return JsonResponse({"error": "Fuel ID already exists"}, status=400)

            vehicle = Vehicles.objects.filter(vehicle_id=vehicle_id).first()
            if not vehicle:
                return JsonResponse({"error": "Vehicle not found"}, status=404)

            trip = Trips.objects.filter(trip_id=trip_id).first()
            if not trip:
                return JsonResponse({"error": "Trip not found"}, status=404)

            # Trip & Vehicle match validation
            if trip.vehicle_id != vehicle_id:
                return JsonResponse({"error": "Vehicle does not match trip"}, status=400)

            # Liters & cost validation
            if float(liters) <= 0:
                return JsonResponse({"error": "Liters must be greater than 0"}, status=400)

            if float(cost) <= 0:
                return JsonResponse({"error": "Cost must be greater than 0"}, status=400)

            fuel_date_obj = datetime.strptime(fuel_date, "%Y-%m-%d").date()

            FuelLogs.objects.create(
                fuel_id=fuel_id,
                vehicle_id=vehicle_id,
                trip_id=trip_id,
                liters=liters,
                cost=cost,
                fuel_date=fuel_date_obj
            )

            return JsonResponse({
                "message": "Fuel log created successfully",
                "fuel_id": fuel_id
            })

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405) 
def get_all_fuel_logs(request):
    logs = FuelLogs.objects.all()
    data = []

    for f in logs:
        data.append({
            "fuel_id": f.fuel_id,
            "vehicle_id": f.vehicle_id,
            "trip_id": f.trip_id,
            "liters": f.liters,
            "cost": f.cost,
            "fuel_date": f.fuel_date
        })

    return JsonResponse({"fuel_logs": data})

@csrf_exempt
def get_fuel_by_vehicle(request):
    if request.method == "POST":
        data = json.loads(request.body)
        vehicle_id = data.get("vehicle_id")

        logs = FuelLogs.objects.filter(vehicle_id=vehicle_id)

        if not logs:
            return JsonResponse({"error": "No fuel logs found"}, status=404)

        data_list = []
        for f in logs:
            data_list.append({
                "fuel_id": f.fuel_id,
                "trip_id": f.trip_id,
                "liters": f.liters,
                "cost": f.cost,
                "fuel_date": f.fuel_date
            })

        return JsonResponse({"fuel_logs": data_list})

    return JsonResponse({"error": "Invalid request method"}, status=405)

@csrf_exempt
def calculate_fuel_efficiency(request):
    if request.method == "POST":
        data = json.loads(request.body)
        trip_id = data.get("trip_id")

        trip = Trips.objects.filter(trip_id=trip_id).first()
        if not trip:
            return JsonResponse({"error": "Trip not found"}, status=404)

        if not trip.end_odometer:
            return JsonResponse({"error": "Trip not completed yet"}, status=400)

        distance = trip.end_odometer - trip.start_odometer

        fuel_logs = FuelLogs.objects.filter(trip_id=trip_id)
        total_liters = sum(float(f.liters) for f in fuel_logs)

        if total_liters == 0:
            return JsonResponse({"error": "No fuel data found"}, status=400)

        efficiency = distance / total_liters

        return JsonResponse({
            "trip_id": trip_id,
            "distance_km": distance,
            "total_liters": total_liters,
            "fuel_efficiency_km_per_liter": round(efficiency, 2)
        })

    return JsonResponse({"error": "Invalid request method"}, status=405)

@csrf_exempt
def create_expense(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)

            expense_id = data.get("expense_id")
            vehicle_id = data.get("vehicle_id")
            expense_type = data.get("expense_type")
            amount = data.get("amount")
            expense_date = data.get("expense_date")
            remarks = data.get("remarks")

            # Required validation
            if not all([expense_id, vehicle_id, expense_type, amount, expense_date]):
                return JsonResponse({"error": "All required fields are mandatory"}, status=400)

            # Duplicate check
            if Expenses.objects.filter(expense_id=expense_id).exists():
                return JsonResponse({"error": "Expense ID already exists"}, status=400)

            vehicle = Vehicles.objects.filter(vehicle_id=vehicle_id).first()
            if not vehicle:
                return JsonResponse({"error": "Vehicle not found"}, status=404)

            # Amount validation
            if float(amount) <= 0:
                return JsonResponse({"error": "Amount must be greater than 0"}, status=400)

            expense_date_obj = datetime.strptime(expense_date, "%Y-%m-%d").date()

            Expenses.objects.create(
                expense_id=expense_id,
                vehicle_id=vehicle_id,
                expense_type=expense_type,
                amount=amount,
                expense_date=expense_date_obj,
                remarks=remarks
            )

            return JsonResponse({
                "message": "Expense created successfully",
                "expense_id": expense_id
            })

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405)

def get_all_expenses(request):
    expenses = Expenses.objects.all()
    data = []

    for e in expenses:
        data.append({
            "expense_id": e.expense_id,
            "vehicle_id": e.vehicle_id,
            "expense_type": e.expense_type,
            "amount": e.amount,
            "expense_date": e.expense_date,
            "remarks": e.remarks
        })

    return JsonResponse({"expenses": data})

@csrf_exempt
def get_expense_by_vehicle(request):
    if request.method == "POST":
        data = json.loads(request.body)
        vehicle_id = data.get("vehicle_id")

        logs = Expenses.objects.filter(vehicle_id=vehicle_id)

        if not logs:
            return JsonResponse({"error": "No expenses found"}, status=404)

        data_list = []
        for e in logs:
            data_list.append({
                "expense_id": e.expense_id,
                "expense_type": e.expense_type,
                "amount": e.amount,
                "expense_date": e.expense_date,
                "remarks": e.remarks
            })

        return JsonResponse({"expenses": data_list})

    return JsonResponse({"error": "Invalid request method"}, status=405)

@csrf_exempt
def calculate_total_expense(request):
    if request.method == "POST":
        data = json.loads(request.body)
        vehicle_id = data.get("vehicle_id")

        vehicle = Vehicles.objects.filter(vehicle_id=vehicle_id).first()
        if not vehicle:
            return JsonResponse({"error": "Vehicle not found"}, status=404)

        expenses = Expenses.objects.filter(vehicle_id=vehicle_id)
        total = sum(float(e.amount) for e in expenses)

        return JsonResponse({
            "vehicle_id": vehicle_id,
            "total_expense": total
        })

    return JsonResponse({"error": "Invalid request method"}, status=405)

@csrf_exempt
def create_revenue(request):
    if request.method == "POST":
        try:
            data = json.loads(request.body)

            revenue_id = data.get("revenue_id")
            trip_id = data.get("trip_id")
            revenue_amount = data.get("revenue_amount")
            received_date = data.get("received_date")

            # Required validation
            if not all([revenue_id, trip_id, revenue_amount, received_date]):
                return JsonResponse({"error": "All required fields are mandatory"}, status=400)

            # Duplicate check
            if Revenue.objects.filter(revenue_id=revenue_id).exists():
                return JsonResponse({"error": "Revenue ID already exists"}, status=400)

            trip = Trips.objects.filter(trip_id=trip_id).first()
            if not trip:
                return JsonResponse({"error": "Trip not found"}, status=404)

            if float(revenue_amount) <= 0:
                return JsonResponse({"error": "Revenue amount must be greater than 0"}, status=400)

            received_date_obj = datetime.strptime(received_date, "%Y-%m-%d").date()

            Revenue.objects.create(
                revenue_id=revenue_id,
                trip_id=trip_id,
                revenue_amount=revenue_amount,
                received_date=received_date_obj
            )

            return JsonResponse({
                "message": "Revenue added successfully",
                "revenue_id": revenue_id
            })

        except Exception as e:
            return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse({"error": "Invalid request method"}, status=405)
def get_all_revenue(request):
    revenues = Revenue.objects.all()
    data = []

    for r in revenues:
        data.append({
            "revenue_id": r.revenue_id,
            "trip_id": r.trip_id,
            "revenue_amount": r.revenue_amount,
            "received_date": r.received_date
        })

    return JsonResponse({"revenues": data})
@csrf_exempt
def get_revenue_by_trip(request):
    if request.method == "POST":
        data = json.loads(request.body)
        trip_id = data.get("trip_id")

        revenues = Revenue.objects.filter(trip_id=trip_id)

        if not revenues:
            return JsonResponse({"error": "No revenue found"}, status=404)

        data_list = []
        for r in revenues:
            data_list.append({
                "revenue_id": r.revenue_id,
                "revenue_amount": r.revenue_amount,
                "received_date": r.received_date
            })

        return JsonResponse({"revenues": data_list})

    return JsonResponse({"error": "Invalid request method"}, status=405)

@csrf_exempt
def calculate_roi(request):
    if request.method == "POST":
        data = json.loads(request.body)
        vehicle_id = data.get("vehicle_id")

        vehicle = Vehicles.objects.filter(vehicle_id=vehicle_id).first()
        if not vehicle:
            return JsonResponse({"error": "Vehicle not found"}, status=404)

        # Total Revenue
        trips = Trips.objects.filter(vehicle_id=vehicle_id)
        total_revenue = 0
        for trip in trips:
            revenues = Revenue.objects.filter(trip_id=trip.trip_id)
            total_revenue += sum(float(r.revenue_amount) for r in revenues)

        # Total Fuel Cost
        total_fuel = sum(float(f.cost) for f in FuelLogs.objects.filter(vehicle_id=vehicle_id))

        # Total Maintenance Cost
        total_maintenance = sum(float(m.cost) for m in MaintenanceLogs.objects.filter(vehicle_id=vehicle_id))

        total_expense = total_fuel + total_maintenance

        if float(vehicle.acquisition_cost) == 0:
            return JsonResponse({"error": "Acquisition cost cannot be zero"}, status=400)

        roi = (total_revenue - total_expense) / float(vehicle.acquisition_cost)

        return JsonResponse({
            "vehicle_id": vehicle_id,
            "total_revenue": total_revenue,
            "total_expense": total_expense,
            "roi": round(roi, 2)
        })

    return JsonResponse({"error": "Invalid request method"}, status=405)
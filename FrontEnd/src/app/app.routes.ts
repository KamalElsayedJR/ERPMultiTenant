import { Routes } from '@angular/router';
import { DashboardComponent } from './features/dashboard/pages/dashboard/dashboard.component';
import { LoginComponent } from './features/auth/pages/login/login.component';
import { RegisterComponent } from './features/auth/pages/register/register.component';
import { authChildGuard, authGuard } from './core/guards/auth.guard';
import { DashboardLayoutComponent } from './layout/dashboard-layout/dashboard-layout.component';
import { UsersListComponent } from './features/users/pages/users-list/users-list.component';
import { DepartmentsListComponent } from './features/departments/pages/departments-list/departments-list.component';
import { EmployeesListComponent } from './features/employees/pages/employees-list/employees-list.component';
import { EmployeeDetailsComponent } from './features/employees/pages/employee-details/employee-details.component';
import { roleGuard } from './core/guards/role.guard';
import { permissionGuard } from './core/guards/permission.guard';
import { ChangePasswordComponent } from './features/auth/pages/change-password/change-password.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: '',
    component: DashboardLayoutComponent,
    canActivate: [authGuard],
    canActivateChild: [authChildGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'change-password', component: ChangePasswordComponent },
      {
        path: 'users',
        component: UsersListComponent,
        canActivate: [roleGuard, permissionGuard],
        data: {
          roles: ['Admin'],
          permissions: ['ManageUsers']
        }
      },
      {
        path: 'departments',
        component: DepartmentsListComponent,
        canActivate: [permissionGuard],
        data: {
          permissions: ['ManageDepartments']
        }
      },
      {
        path: 'employees',
        component: EmployeesListComponent,
        canActivate: [permissionGuard],
        data: {
          permissions: ['ManageEmployees']
        }
      },
      {
        path: 'employees/:employeeId',
        component: EmployeeDetailsComponent,
        canActivate: [permissionGuard],
        data: {
          permissions: ['ManageEmployees']
        }
      }
    ]
  }
];

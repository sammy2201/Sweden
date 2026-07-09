import { Routes } from "@angular/router";
import { PNumberComponent } from "./pages/p-number/p-number.component";
import { HomeComponent } from "./pages/home/home.component";
import { HousingComponent } from "./pages/housing/housing.component";
import { RoadmapComponent } from "./pages/roadmap/roadmap.component";
import { LoginComponent } from "./pages/login/login.component";
import { RegisterComponent } from "./pages/register/register.component";

export const routes: Routes = [
  { path: "p-number", component: PNumberComponent },
  { path: "", component: HomeComponent },
  { path: "housing", component: HousingComponent },
  { path: "roadmap", component: RoadmapComponent },
  { path: "login", component: LoginComponent },
  { path: "register", component: RegisterComponent },
];

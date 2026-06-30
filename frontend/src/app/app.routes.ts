import { Routes } from "@angular/router";
import { PNumberComponent } from "./pages/p-number/p-number.component";
import { HomeComponent } from "./pages/home/home.component";
import { HousingComponent } from "./pages/housing/housing.component";

export const routes: Routes = [
  { path: "p-number", component: PNumberComponent },
  { path: "", component: HomeComponent },
  { path: "housing", component: HousingComponent },
];

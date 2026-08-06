import { Routes } from "@angular/router";
import { PNumberComponent } from "./pages/p-number/p-number.component";
import { HomeComponent } from "./pages/home/home.component";
import { HousingComponent } from "./pages/housing/housing.component";
import { RoadmapComponent } from "./pages/roadmap/roadmap.component";
import { LoginComponent } from "./pages/login/login.component";
import { RegisterComponent } from "./pages/register/register.component";
import { DigitalIDComponent } from "./pages/digitalID/digital-ID.component";
import { BankingComponent } from "./pages/banking/banking.component";
import { TaxInfoComponent } from "./pages/tax/tax.component";
import { authGuard } from "./guards/auth.guard";
import { TaxCalculationComponent } from "./pages/tax-calculation/tax-calculation.component";
import { TransportComponent } from "./pages/transport/transport.component";
import { AttractionDetailsComponent } from "./pages/explore/components/attraction-details.component";
import { CountyAttractionsComponent } from "./pages/explore/components/county-attractions.component";
import { ExploreCountiesComponent } from "./pages/explore/components/explore-counties.component";
import { ProfileComponent } from "./pages/profile/profile.component";

export const routes: Routes = [
  { path: "p-number", component: PNumberComponent },
  { path: "", component: HomeComponent },
  { path: "housing", component: HousingComponent },
  { path: "explore", component: ExploreCountiesComponent },
  {
    path: "explore/:countyId/:attractionId",
    component: AttractionDetailsComponent,
  },
  { path: "explore/:countyId", component: CountyAttractionsComponent },
  { path: "roadmap", component: RoadmapComponent },
  { path: "login", component: LoginComponent },
  { path: "register", component: RegisterComponent },
  { path: "banking", component: BankingComponent, canActivate: [authGuard] },
  { path: "profile", component: ProfileComponent, canActivate: [authGuard] },
  { path: "tax-info", component: TaxInfoComponent, canActivate: [authGuard] },
  { path: "digital-id", component: DigitalIDComponent },
  {
    path: "tax-calculation",
    component: TaxCalculationComponent,
    canActivate: [authGuard],
  },
  {
    path: "transport",
    component: TransportComponent,
    canActivate: [authGuard],
  },
];

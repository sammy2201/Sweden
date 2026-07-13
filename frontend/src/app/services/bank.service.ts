import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Bank } from "../pages/banking/bank.model";
import { environment } from "../../environments/environment";

@Injectable({
  providedIn: "root",
})
export class BankService {
  private http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/bank-details`;

  getBanks(): Observable<Bank[]> {
    return this.http.get<Bank[]>(this.apiUrl);
  }
}

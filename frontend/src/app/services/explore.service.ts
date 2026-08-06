import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { environment } from "../../environments/environment";
import {
  Attraction,
  AttractionDetail,
  County,
  PagedResponse,
} from "../models/explore.models";

interface ExploreCountiesResponseDto {
  counties: County[];
}

interface ExploreAttractionsResponseDto {
  attractions: Attraction[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

interface ExploreAttractionDetailResponseDto {
  attraction: AttractionDetail;
}

@Injectable({
  providedIn: "root",
})
export class ExploreService {
  constructor(private readonly http: HttpClient) {}

  getCounties(): Observable<County[]> {
    return this.http
      .get<ExploreCountiesResponseDto>(`${environment.apiUrl}/explore/counties`)
      .pipe(map((response) => response.counties ?? []));
  }

  getCounty(countyId: string): Observable<County | undefined> {
    return this.getCounties().pipe(
      map((counties) => counties.find((county) => county.id === countyId)),
    );
  }

  getAttractions(
    county: string,
    page = 1,
    pageSize = 12,
    category?: string,
  ): Observable<PagedResponse<Attraction>> {
    let params = new HttpParams()
      .set("county", county)
      .set("page", page)
      .set("pageSize", pageSize);

    const cleanedCategory = category?.trim();
    if (cleanedCategory) {
      params = params.set("category", cleanedCategory);
    }

    return this.http
      .get<ExploreAttractionsResponseDto>(
        `${environment.apiUrl}/explore/attractions`,
        {
          params,
        },
      )
      .pipe(
        map((response) => ({
          items: response.attractions ?? [],
          page: response.page,
          pageSize: response.pageSize,
          totalCount: response.totalCount,
          totalPages: response.totalPages,
          hasNextPage: response.hasNextPage,
          hasPreviousPage: response.hasPreviousPage,
        })),
      );
  }

  getAttraction(
    _countyId: string,
    attractionId: string,
  ): Observable<AttractionDetail | undefined> {
    return this.http
      .get<ExploreAttractionDetailResponseDto>(
        `${environment.apiUrl}/explore/attractions/${encodeURIComponent(attractionId)}`,
      )
      .pipe(map((response) => response.attraction));
  }
}

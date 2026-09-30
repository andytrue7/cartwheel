import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { PagedResponse, ProductResponse } from './product.models';

@Injectable({
  providedIn: 'root',
})
export class ProductsApi {
  private readonly http = inject(HttpClient);

  getProducts(): Observable<PagedResponse<ProductResponse>> {
    return this.http.get<PagedResponse<ProductResponse>>(`${environment.apiUrl}/api/products`);
  }
}

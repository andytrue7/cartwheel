import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { ProductsApi } from './products-api';

describe('ProductsApi', () => {
  let service: ProductsApi;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ProductsApi);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

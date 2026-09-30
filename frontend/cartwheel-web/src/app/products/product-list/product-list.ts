import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { ProductResponse } from '../product.models';
import { ProductsApi } from '../products-api';

@Component({
  selector: 'app-product-list',
  imports: [CurrencyPipe],
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss',
})
export class ProductList {
  private readonly api = inject(ProductsApi);

  protected readonly products = signal<readonly ProductResponse[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.api
      .getProducts()
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (page) => {
          this.products.set(page.items);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Could not load products.');
          this.loading.set(false);
        },
      });
  }
}

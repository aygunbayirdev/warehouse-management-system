import { createBrowserRouter } from 'react-router-dom'

import { LoginPage } from '@/features/auth/LoginPage'
import { GoodsReceiptsPage } from '@/features/inbound/GoodsReceiptsPage'
import { NewGoodsReceiptPage } from '@/features/inbound/NewGoodsReceiptPage'
import { GoodsIssuesPage } from '@/features/outbound/GoodsIssuesPage'
import { NewGoodsIssuePage } from '@/features/outbound/NewGoodsIssuePage'
import { CategoriesPage } from '@/features/products/CategoriesPage'
import { ProductsPage } from '@/features/products/ProductsPage'
import { UnitsOfMeasurePage } from '@/features/products/UnitsOfMeasurePage'
import { ReportsPage } from '@/features/reports/ReportsPage'
import { StockCountAdjustmentsPage } from '@/features/stock-count/StockCountAdjustmentsPage'
import { StockCountDetailPage } from '@/features/stock-count/StockCountDetailPage'
import { StockCountsPage } from '@/features/stock-count/StockCountsPage'
import { NewStockTransferPage } from '@/features/transfer/NewStockTransferPage'
import { StockTransfersPage } from '@/features/transfer/StockTransfersPage'
import { WarehousesPage } from '@/features/warehouses/WarehousesPage'

import { AppLayout } from './layout/AppLayout'
import { DashboardPage } from './routes/DashboardPage'
import { ProtectedRoute } from './routes/ProtectedRoute'

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { path: '/', element: <DashboardPage /> },
          { path: '/products', element: <ProductsPage /> },
          { path: '/categories', element: <CategoriesPage /> },
          { path: '/units-of-measure', element: <UnitsOfMeasurePage /> },
          { path: '/warehouses', element: <WarehousesPage /> },
          { path: '/goods-receipts', element: <GoodsReceiptsPage /> },
          { path: '/goods-receipts/new', element: <NewGoodsReceiptPage /> },
          { path: '/goods-issues', element: <GoodsIssuesPage /> },
          { path: '/goods-issues/new', element: <NewGoodsIssuePage /> },
          { path: '/stock-transfers', element: <StockTransfersPage /> },
          { path: '/stock-transfers/new', element: <NewStockTransferPage /> },
          { path: '/stock-counts', element: <StockCountsPage /> },
          { path: '/stock-counts/:id', element: <StockCountDetailPage /> },
          {
            path: '/stock-count-adjustments',
            element: <StockCountAdjustmentsPage />,
          },
          { path: '/reports', element: <ReportsPage /> },
        ],
      },
    ],
  },
])

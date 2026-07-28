import { createBrowserRouter } from 'react-router-dom'

import { LoginPage } from '@/features/auth/LoginPage'
import { RoleNames } from '@/features/auth/types'
import { CategoriesPage } from '@/features/products/CategoriesPage'
import { ProductsPage } from '@/features/products/ProductsPage'
import { UnitsOfMeasurePage } from '@/features/products/UnitsOfMeasurePage'
import { WarehousesPage } from '@/features/warehouses/WarehousesPage'

import { AppLayout } from './layout/AppLayout'
import { AdminPlaceholderPage } from './routes/AdminPlaceholderPage'
import { DashboardPage } from './routes/DashboardPage'
import { ProtectedRoute } from './routes/ProtectedRoute'
import { RoleGuard } from './routes/RoleGuard'

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
          {
            path: '/admin',
            element: (
              <RoleGuard allowedRoles={[RoleNames.Admin]}>
                <AdminPlaceholderPage />
              </RoleGuard>
            ),
          },
        ],
      },
    ],
  },
])

import { createBrowserRouter } from 'react-router-dom'

import { LoginPage } from '@/features/auth/LoginPage'
import { RoleNames } from '@/features/auth/types'

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
        path: '/',
        element: <DashboardPage />,
      },
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
])

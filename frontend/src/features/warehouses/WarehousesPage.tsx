import { Pencil, Plus, Trash2 } from 'lucide-react'
import { type FormEvent, useState } from 'react'
import { toast } from 'sonner'

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogTitle,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { useHasAnyRole } from '@/features/auth/api/useHasAnyRole'
import { RoleNames } from '@/features/auth/types'
import { getApiErrorMessage } from '@/lib/errors'

import {
  useCreateWarehouse,
  useDeleteWarehouse,
  useUpdateWarehouse,
  useWarehouses,
} from './api/warehouses'
import type { WarehouseDto } from './types'

export function WarehousesPage() {
  const { data: warehouses, isLoading } = useWarehouses()
  const canManage = useHasAnyRole([RoleNames.Admin, RoleNames.WarehouseManager])

  const [editingWarehouse, setEditingWarehouse] =
    useState<WarehouseDto | null>(null)
  const [isFormOpen, setIsFormOpen] = useState(false)
  const [deletingWarehouse, setDeletingWarehouse] =
    useState<WarehouseDto | null>(null)

  function openCreateForm() {
    setEditingWarehouse(null)
    setIsFormOpen(true)
  }

  function openEditForm(warehouse: WarehouseDto) {
    setEditingWarehouse(warehouse)
    setIsFormOpen(true)
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">Depolar</h2>
        {canManage && (
          <Button onClick={openCreateForm}>
            <Plus /> Yeni Depo
          </Button>
        )}
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Kod</TableHead>
            <TableHead>Ad</TableHead>
            <TableHead>Adres</TableHead>
            {canManage && <TableHead className="w-24">Aksiyonlar</TableHead>}
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading && (
            <TableRow>
              <TableCell colSpan={4}>Yükleniyor...</TableCell>
            </TableRow>
          )}
          {warehouses?.map((warehouse) => (
            <TableRow key={warehouse.id}>
              <TableCell>{warehouse.code}</TableCell>
              <TableCell>{warehouse.name}</TableCell>
              <TableCell>{warehouse.address ?? '—'}</TableCell>
              {canManage && (
                <TableCell>
                  <div className="flex gap-1">
                    <Button
                      variant="ghost"
                      size="icon-sm"
                      aria-label="Düzenle"
                      onClick={() => openEditForm(warehouse)}
                    >
                      <Pencil />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon-sm"
                      aria-label="Sil"
                      onClick={() => setDeletingWarehouse(warehouse)}
                    >
                      <Trash2 />
                    </Button>
                  </div>
                </TableCell>
              )}
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <WarehouseFormDialog
        key={editingWarehouse?.id ?? 'new'}
        open={isFormOpen}
        onOpenChange={setIsFormOpen}
        warehouse={editingWarehouse}
      />

      <DeleteWarehouseDialog
        warehouse={deletingWarehouse}
        onOpenChange={(open) => {
          if (!open) setDeletingWarehouse(null)
        }}
      />
    </div>
  )
}

function WarehouseFormDialog({
  open,
  onOpenChange,
  warehouse,
}: {
  open: boolean
  onOpenChange: (open: boolean) => void
  warehouse: WarehouseDto | null
}) {
  const [code, setCode] = useState(warehouse?.code ?? '')
  const [name, setName] = useState(warehouse?.name ?? '')
  const [address, setAddress] = useState(warehouse?.address ?? '')

  const createWarehouse = useCreateWarehouse()
  const updateWarehouse = useUpdateWarehouse()
  const mutation = warehouse ? updateWarehouse : createWarehouse

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (warehouse) {
      updateWarehouse.mutate(
        { id: warehouse.id, payload: { name, address: address || null } },
        {
          onSuccess: () => {
            toast.success('Depo güncellendi.')
            onOpenChange(false)
          },
        },
      )
    } else {
      createWarehouse.mutate(
        { code, name, address: address || null },
        {
          onSuccess: () => {
            toast.success('Depo oluşturuldu.')
            onOpenChange(false)
          },
        },
      )
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          <DialogTitle>{warehouse ? 'Depoyu Düzenle' : 'Yeni Depo'}</DialogTitle>

          <div className="space-y-1.5">
            <Label htmlFor="warehouse-code">Kod</Label>
            <Input
              id="warehouse-code"
              required
              maxLength={20}
              disabled={Boolean(warehouse)}
              value={code}
              onChange={(event) => setCode(event.target.value)}
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="warehouse-name">Ad</Label>
            <Input
              id="warehouse-name"
              required
              maxLength={150}
              value={name}
              onChange={(event) => setName(event.target.value)}
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="warehouse-address">Adres</Label>
            <Input
              id="warehouse-address"
              maxLength={300}
              value={address}
              onChange={(event) => setAddress(event.target.value)}
            />
          </div>

          {mutation.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(mutation.error)}
            </p>
          )}

          <DialogFooter>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? 'Kaydediliyor...' : 'Kaydet'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function DeleteWarehouseDialog({
  warehouse,
  onOpenChange,
}: {
  warehouse: WarehouseDto | null
  onOpenChange: (open: boolean) => void
}) {
  const deleteWarehouse = useDeleteWarehouse()

  function handleConfirm() {
    if (!warehouse) return

    deleteWarehouse.mutate(warehouse.id, {
      onSuccess: () => {
        toast.success('Depo silindi.')
        onOpenChange(false)
      },
      onError: (error) => {
        toast.error(getApiErrorMessage(error))
        onOpenChange(false)
      },
    })
  }

  return (
    <AlertDialog open={warehouse !== null} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogTitle>Depoyu sil</AlertDialogTitle>
        <AlertDialogDescription>
          &quot;{warehouse?.name}&quot; deposunu silmek istediğinize emin
          misiniz?
        </AlertDialogDescription>
        <AlertDialogFooter>
          <AlertDialogCancel>Vazgeç</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleConfirm}
            disabled={deleteWarehouse.isPending}
          >
            Sil
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}

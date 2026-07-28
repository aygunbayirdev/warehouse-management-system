import { Pencil, Plus, Trash2 } from 'lucide-react'
import { type FormEvent, useState } from 'react'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
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
  useCreateUnitOfMeasure,
  useDeleteUnitOfMeasure,
  useUnitsOfMeasure,
  useUpdateUnitOfMeasure,
} from './api/unitsOfMeasure'
import type { UnitOfMeasureDto } from './types'

export function UnitsOfMeasurePage() {
  const { data: units, isLoading } = useUnitsOfMeasure()
  const canManage = useHasAnyRole([RoleNames.Admin, RoleNames.WarehouseManager])

  const [editingUnit, setEditingUnit] = useState<UnitOfMeasureDto | null>(null)
  const [isFormOpen, setIsFormOpen] = useState(false)
  const [deletingUnit, setDeletingUnit] = useState<UnitOfMeasureDto | null>(
    null,
  )

  function openCreateForm() {
    setEditingUnit(null)
    setIsFormOpen(true)
  }

  function openEditForm(unit: UnitOfMeasureDto) {
    setEditingUnit(unit)
    setIsFormOpen(true)
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">Birimler</h2>
        {canManage && (
          <Button onClick={openCreateForm}>
            <Plus /> Yeni Birim
          </Button>
        )}
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Kod</TableHead>
            <TableHead>Ad</TableHead>
            {canManage && <TableHead className="w-24">Aksiyonlar</TableHead>}
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading && (
            <TableRow>
              <TableCell colSpan={3}>Yükleniyor...</TableCell>
            </TableRow>
          )}
          {units?.map((unit) => (
            <TableRow key={unit.id}>
              <TableCell>{unit.code}</TableCell>
              <TableCell>{unit.name}</TableCell>
              {canManage && (
                <TableCell>
                  <div className="flex gap-1">
                    <Button
                      variant="ghost"
                      size="icon-sm"
                      aria-label="Düzenle"
                      onClick={() => openEditForm(unit)}
                    >
                      <Pencil />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon-sm"
                      aria-label="Sil"
                      onClick={() => setDeletingUnit(unit)}
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

      <UnitOfMeasureFormDialog
        key={editingUnit?.id ?? 'new'}
        open={isFormOpen}
        onOpenChange={setIsFormOpen}
        unit={editingUnit}
      />

      <DeleteUnitOfMeasureDialog
        unit={deletingUnit}
        onOpenChange={(open) => {
          if (!open) setDeletingUnit(null)
        }}
      />
    </div>
  )
}

function UnitOfMeasureFormDialog({
  open,
  onOpenChange,
  unit,
}: {
  open: boolean
  onOpenChange: (open: boolean) => void
  unit: UnitOfMeasureDto | null
}) {
  const [code, setCode] = useState(unit?.code ?? '')
  const [name, setName] = useState(unit?.name ?? '')
  const createUnit = useCreateUnitOfMeasure()
  const updateUnit = useUpdateUnitOfMeasure()
  const mutation = unit ? updateUnit : createUnit

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const payload = { code, name }

    if (unit) {
      updateUnit.mutate(
        { id: unit.id, payload },
        {
          onSuccess: () => {
            toast.success('Birim güncellendi.')
            onOpenChange(false)
          },
        },
      )
    } else {
      createUnit.mutate(payload, {
        onSuccess: () => {
          toast.success('Birim oluşturuldu.')
          onOpenChange(false)
        },
      })
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          <DialogTitle>{unit ? 'Birimi Düzenle' : 'Yeni Birim'}</DialogTitle>

          <div className="space-y-1.5">
            <Label htmlFor="unit-code">Kod</Label>
            <Input
              id="unit-code"
              required
              maxLength={10}
              value={code}
              onChange={(event) => setCode(event.target.value)}
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="unit-name">Ad</Label>
            <Input
              id="unit-name"
              required
              maxLength={100}
              value={name}
              onChange={(event) => setName(event.target.value)}
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

function DeleteUnitOfMeasureDialog({
  unit,
  onOpenChange,
}: {
  unit: UnitOfMeasureDto | null
  onOpenChange: (open: boolean) => void
}) {
  const deleteUnit = useDeleteUnitOfMeasure()

  function handleConfirm() {
    if (!unit) return

    deleteUnit.mutate(unit.id, {
      onSuccess: () => {
        toast.success('Birim silindi.')
        onOpenChange(false)
      },
      onError: (error) => {
        toast.error(getApiErrorMessage(error))
        onOpenChange(false)
      },
    })
  }

  return (
    <AlertDialog open={unit !== null} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogTitle>Birimi sil</AlertDialogTitle>
        <AlertDialogDescription>
          &quot;{unit?.name}&quot; birimini silmek istediğinize emin misiniz?
        </AlertDialogDescription>
        <AlertDialogFooter>
          <AlertDialogCancel>Vazgeç</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleConfirm}
            disabled={deleteUnit.isPending}
          >
            Sil
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}

export type StaffTypeValue = 'Photographer' | 'Journalist' | 'Tv'

export interface StaffTypeInfo {
  value: StaffTypeValue
  label: string
  description: string
  // Colors used by the role logo / badge (soft background + strong foreground)
  color: string
  bg: string
}

export const STAFF_TYPES: readonly StaffTypeInfo[] = [
  {
    value: 'Photographer',
    label: 'Photographer',
    description: 'Cover matches with photos',
    color: '#1d4ed8',
    bg: '#e8f0ff',
  },
  {
    value: 'Journalist',
    label: 'Journalist',
    description: 'Report and write about matches',
    color: '#6d28d9',
    bg: '#f1eaff',
  },
  {
    value: 'Tv',
    label: 'TV',
    description: 'Film or broadcast matches',
    color: '#0f766e',
    bg: '#dff5f1',
  },
]

export function staffTypeInfo(value: string | null | undefined): StaffTypeInfo | undefined {
  return STAFF_TYPES.find((t) => t.value === value)
}

export function staffTypeLabel(value: string | null | undefined): string {
  return staffTypeInfo(value)?.label ?? 'Not chosen'
}

import { STAFF_TYPES } from '../constants/staffTypes'
import { RoleIcon } from './RoleIcon'

interface Props {
  value: string | null
  onChange: (value: string) => void
  disabled?: boolean
}

export default function StaffTypePicker({ value, onChange, disabled }: Props) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
      {STAFF_TYPES.map((type) => {
        const selected = value === type.value
        return (
          <label
            key={type.value}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: 12,
              padding: '12px 14px',
              borderRadius: 10,
              cursor: disabled ? 'not-allowed' : 'pointer',
              border: selected ? `1.5px solid ${type.color}` : '1px solid var(--border)',
              background: selected ? type.bg : 'var(--surface)',
              opacity: disabled ? 0.6 : 1,
            }}
          >
            <input
              type="radio"
              name="staffType"
              value={type.value}
              checked={selected}
              disabled={disabled}
              onChange={() => onChange(type.value)}
              style={{ width: 16, height: 16, padding: 0, margin: 0, accentColor: type.color }}
            />
            <RoleIcon type={type.value} size={36} />
            <span>
              <span style={{ display: 'block', fontSize: 14, fontWeight: 600 }}>{type.label}</span>
              <span style={{ display: 'block', fontSize: 12, color: 'var(--text-muted)' }}>
                {type.description}
              </span>
            </span>
          </label>
        )
      })}
    </div>
  )
}

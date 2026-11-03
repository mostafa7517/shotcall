import { staffTypeInfo, staffTypeLabel } from '../constants/staffTypes'

interface RoleIconProps {
  type: string | null | undefined
  size?: number
}

// The drawing for each role. All icons are 24x24 line icons that use currentColor.
function Glyph({ type }: { type: string | null | undefined }) {
  switch (type) {
    case 'Photographer':
      // Camera
      return (
        <>
          <path d="M4 8h3l1.5-2h7L17 8h3a1 1 0 0 1 1 1v9a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V9a1 1 0 0 1 1-1z" />
          <circle cx="12" cy="13" r="3.5" />
        </>
      )
    case 'Journalist':
      // Pen writing on a line
      return (
        <>
          <path d="M16.5 3.5a2.12 2.12 0 0 1 3 3L8 18l-4.5 1L4.5 14.5 16.5 3.5z" />
          <path d="M13 20.5h8" />
        </>
      )
    case 'Tv':
      // Television with antenna
      return (
        <>
          <rect x="3" y="8" width="18" height="12" rx="2" />
          <path d="M8 3.5 12 8l4-4.5" />
        </>
      )
    default:
      // Not chosen yet
      return (
        <>
          <circle cx="12" cy="12" r="9" />
          <path d="M9.5 9.5a2.5 2.5 0 1 1 3.5 2.3c-.7.4-1 .9-1 1.7" />
          <path d="M12 17h.01" />
        </>
      )
  }
}

// The role "logo": a rounded colored tile with the role drawing inside it.
export function RoleIcon({ type, size = 32 }: RoleIconProps) {
  const info = staffTypeInfo(type)
  const color = info?.color ?? '#6b7280'
  const bg = info?.bg ?? '#eef0f2'

  return (
    <span
      role="img"
      aria-label={staffTypeLabel(type)}
      title={staffTypeLabel(type)}
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        justifyContent: 'center',
        flexShrink: 0,
        width: size,
        height: size,
        borderRadius: Math.round(size * 0.28),
        background: bg,
        color,
      }}
    >
      <svg
        width={Math.round(size * 0.62)}
        height={Math.round(size * 0.62)}
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        strokeWidth={1.8}
        strokeLinecap="round"
        strokeLinejoin="round"
        aria-hidden="true"
      >
        <Glyph type={info?.value} />
      </svg>
    </span>
  )
}

// A small pill: logo + role name. Used in lists and tables.
export function RoleBadge({ type, size = 22 }: RoleIconProps) {
  const info = staffTypeInfo(type)

  return (
    <span
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: 6,
        padding: '2px 10px 2px 3px',
        borderRadius: 999,
        background: info?.bg ?? '#eef0f2',
        color: info?.color ?? '#6b7280',
        fontSize: 12,
        fontWeight: 600,
        whiteSpace: 'nowrap',
      }}
    >
      <span style={{ display: 'inline-flex', background: '#fff', borderRadius: '50%' }}>
        <RoleIcon type={type} size={size} />
      </span>
      {staffTypeLabel(type)}
    </span>
  )
}

export default RoleIcon

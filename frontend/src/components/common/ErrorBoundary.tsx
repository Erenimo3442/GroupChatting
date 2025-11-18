import type { ReactNode } from 'react';

interface Props {
  children: ReactNode;
}

// Placeholder component retained for compatibility; simply renders its children.
export default function ErrorBoundary({ children }: Props) {
  return <>{children}</>;
}

import clsx from 'clsx';
import { forwardRef } from 'react';

export type InputProps = { compact?: boolean } & React.DetailedHTMLProps<
  React.InputHTMLAttributes<HTMLInputElement>,
  HTMLInputElement
>;

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ compact = false, className, ...props }, ref) => {
    return (
      <input
        {...props}
        ref={ref}
        className={clsx(
          className,
          'rounded border border-gray-600 bg-gray-800 text-gray-50 transition-colors focus-visible:border-obsidian-500 focus-visible:outline-none',
          compact ? 'px-1' : 'p-1 px-2',
        )}
      />
    );
  },
);

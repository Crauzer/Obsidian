import * as RadixLabel from '@radix-ui/react-label';
import clsx from 'clsx';
import { forwardRef } from 'react';
import React from 'react';

export type InputProps = {
  label?: string;
  compact?: boolean;
  error?: boolean | string;
} & React.DetailedHTMLProps<React.InputHTMLAttributes<HTMLInputElement>, HTMLInputElement>;

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, compact = false, error, className, ...props }, ref) => {
    return (
      <div className={clsx(className, 'flex flex-col gap-1')}>
        {label && (
          <RadixLabel.Root
            className={clsx('text-sm text-gray-50', { 'text-obsidian-500': error })}
            htmlFor={props.id}
          >
            {label}:
          </RadixLabel.Root>
        )}
        <input
          {...props}
          ref={ref}
          className={clsx(
            'rounded border border-gray-500 bg-gray-700 text-gray-50 transition-colors focus-visible:border-obsidian-500 focus-visible:outline-none',
            compact ? 'px-1' : 'p-1 px-2',
            { 'border-obsidian-500': error },
          )}
        />
        {error && typeof error === 'string' && <p className="text-sm text-obsidian-500">{error}</p>}
      </div>
    );
  },
);

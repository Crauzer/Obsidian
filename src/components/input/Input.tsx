import * as RadixLabel from '@radix-ui/react-label';
import clsx from 'clsx';
import { forwardRef, useState } from 'react';
import React from 'react';

export type InputProps = {
  label?: string;
  compact?: boolean;
  error?: boolean | string;

  right?: React.ReactNode;
  left?: React.ReactNode;
} & React.DetailedHTMLProps<React.InputHTMLAttributes<HTMLInputElement>, HTMLInputElement>;

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, compact = false, error, onFocus, onBlur, right, left, className, ...props }, ref) => {
    const [isFocused, setIsFocused] = useState(false);

    return (
      <div className={clsx(className, 'flex flex-col gap-1')}>
        {label && (
          <RadixLabel.Root
            className={clsx('text-base text-gray-50', { 'text-obsidian-500': error })}
            htmlFor={props.id}
          >
            {label}:
          </RadixLabel.Root>
        )}

        <div
          className={clsx(
            'flex h-full w-full flex-row items-center gap-1 rounded-md border border-gray-500 bg-gray-700 px-2 shadow-inner transition-colors',
            {
              'border-obsidian-500/70': isFocused || error,
            },
          )}
        >
          {left}
          <input
            {...props}
            ref={ref}
            className={clsx(
              'h-full w-full rounded bg-gray-700 text-gray-50 focus-visible:outline-none',
              compact ? 'px-1' : 'p-1 px-2',
            )}
            onFocus={(e) => {
              setIsFocused(true);
              onFocus?.(e);
            }}
            onBlur={(e) => {
              setIsFocused(false);
              onBlur?.(e);
            }}
          />
          {right}
        </div>
        {error && typeof error === 'string' && <p className="text-sm text-obsidian-500">{error}</p>}
      </div>
    );
  },
);

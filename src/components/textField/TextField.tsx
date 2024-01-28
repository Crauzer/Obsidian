import * as RadixLabel from '@radix-ui/react-label';
import clsx from 'clsx';
import React, { useRef, useState } from 'react';

export type TextFieldProps = {
  label?: React.ReactNode;
  labelPostfix?: React.ReactNode;
  error?: boolean | string;

  right?: React.ReactNode;
  left?: React.ReactNode;
} & React.DetailedHTMLProps<React.InputHTMLAttributes<HTMLInputElement>, HTMLInputElement>;

export const TextField: React.FC<TextFieldProps> = ({
  label,
  labelPostfix = ':',
  error,
  right,
  left,
  className,
  onBlur,
  onFocus,
  ...props
}) => {
  const inputRef = useRef<HTMLInputElement>(null);

  const [isFocused, setIsFocused] = useState(false);

  return (
    <div className={clsx('flex flex-col gap-1', className)}>
      {label && (
        <RadixLabel.Root
          className={clsx('text-base text-gray-50', { 'text-obsidian-500': error })}
          htmlFor={props.id}
        >
          {label}
          {labelPostfix}
        </RadixLabel.Root>
      )}
      <div
        className={clsx(
          'flex flex-row items-center gap-1 rounded-md border border-gray-500 bg-gray-700 shadow-inner transition-colors',
          {
            'border-obsidian-500/70': isFocused,
          },
        )}
      >
        {left}
        <input
          {...props}
          ref={inputRef}
          className="w-full border-none bg-transparent text-lg focus-visible:border-none focus-visible:outline-none"
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
};

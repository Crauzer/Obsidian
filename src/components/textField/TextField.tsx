import clsx from 'clsx';
import React, { useRef, useState } from 'react';

export type TextFieldProps = {
  right?: React.ReactNode;
  left?: React.ReactNode;

  children?: React.ReactNode;
};

export const TextField: React.FC<TextFieldProps> = ({ right, left }) => {
  const inputRef = useRef<HTMLInputElement>(null);

  const [isFocused, setIsFocused] = useState(false);

  return (
    <div
      className={clsx(
        'flex flex-row items-center gap-1 rounded-md border border-gray-500 bg-gray-700 transition-colors',
        {
          'border-obsidian-500/70': isFocused,
        },
      )}
    >
      {left}
      <input
        ref={inputRef}
        className="w-full border-none bg-transparent text-lg focus-visible:border-none focus-visible:outline-none"
        onFocus={(e) => {
          setIsFocused(true);
        }}
        onBlur={(e) => {
          setIsFocused(false);
        }}
      />
      {right}
    </div>
  );
};

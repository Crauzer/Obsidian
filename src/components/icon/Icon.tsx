import clsx from 'clsx';
import React from 'react';
import { match } from 'ts-pattern';

import { Size } from '../../types';

export type IconProps = {
  size?: Size;

  icon: React.ComponentType<React.SVGProps<SVGSVGElement> & { title?: string | undefined }>;

  className?: string;
} & React.SVGProps<SVGSVGElement>;

export const Icon: React.FC<IconProps> = ({
  size = 'md',
  icon: IconComponent,
  className,
  ...props
}) => {
  return (
    <IconComponent
      {...props}
      className={clsx(className, getSizeClass(size))}
      width={getSizeWidth(size)}
      height={getSizeWidth(size)}
    />
  );
};

const getSizeClass = (size: Size) =>
  match(size)
    .with('xs', () => clsx('w-[12px] h-[12px]'))
    .with('sm', () => clsx('w-[14px] h-[14px]'))
    .with('md', () => clsx('w-[16px] h-[16px]'))
    .with('lg', () => clsx('w-[20px] h-[20px]'))
    .with('xl', () => clsx('w-[32px] h-[32px]'))
    .exhaustive();

const getSizeWidth = (size: Size) =>
  match(size)
    .with('xs', () => 12)
    .with('sm', () => 14)
    .with('md', () => 16)
    .with('lg', () => 20)
    .with('xl', () => 32)
    .exhaustive();

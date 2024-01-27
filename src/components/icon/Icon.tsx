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
    .with('xs', () => clsx('w-[14px] h-[14px]'))
    .with('sm', () => clsx('w-[16px] h-[16px]'))
    .with('md', () => clsx('w-[20px] h-[20px]'))
    .with('lg', () => clsx('w-[32px] h-[32px]'))
    .with('xl', () => clsx('w-[40px] h-[40px]'))
    .exhaustive();

const getSizeWidth = (size: Size) =>
  match(size)
    .with('xs', () => 14)
    .with('sm', () => 16)
    .with('md', () => 20)
    .with('lg', () => 32)
    .with('xl', () => 40)
    .exhaustive();

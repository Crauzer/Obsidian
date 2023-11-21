import React from 'react';

type BasePolymorphicProps<TAs extends React.ElementType, TProps = {}> = {
  as?: TAs;
} & TProps;

export type PolymorphicProps<TAs extends React.ElementType, TProps = {}> = React.PropsWithChildren<
  BasePolymorphicProps<TAs, TProps>
> &
  Omit<React.ComponentPropsWithoutRef<TAs>, keyof BasePolymorphicProps<TAs, TProps>>;

export type PolymorphicPropsWithRef<TAs extends React.ElementType, TProps = {}> = PolymorphicProps<
  TAs,
  TProps
> & { ref?: ElementRef<TAs> };

export type ElementRef<TElement extends React.ElementType> =
  React.ComponentPropsWithRef<TElement>['ref'];

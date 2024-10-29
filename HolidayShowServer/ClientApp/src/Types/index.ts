export * from './DisposableBase'

export interface IDisposable {
    dispose: ()=>void;
}

export interface IInitializable{
    initialize: ()=>void;
}